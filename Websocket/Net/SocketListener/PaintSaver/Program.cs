using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SimpleJson;
using SocketListener;
using Microsoft.Kinect;

namespace PaintSaver
{
    class Program : IDisposable
    {
        KinectSensor kinect;
        Boolean isRecording = false;
        private StreamWriter fileWriter;
        CometdSocket socket;
        //ColorImageFrame imageFrame;
        //DepthImageFrame depthFrame;
        //minimal change in mm to send a new hand position

        String handPositionChannel = "/datachannel/handPosition";
        String drawPositionChannel = "/datachannel/drawPosition";
        String eyePositionChannel = "/datachannel/eyePosition";
        String lookAtChannel = "/datachannel/lookAt";
        String touchChannel = "/datachannel/touch";
        PointBuffer handPositionBuffer = new PointBuffer(4, "handPositionBuffer");
        PointBuffer eyePositionBuffer = new PointBuffer(8, "eyePositionBuffer");
        PointBuffer lookAtBuffer = new PointBuffer(8, "lookAtBuffer");

        Body[] skeletons;


        List<CameraSpacePoint> allPoints = new List<CameraSpacePoint>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            using (Program pro = new Program())
            {
                pro.start();
                Console.ReadLine();
            }

        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e);
        }

        public Program()
        {
            //Console.WriteLine(Path.GetFullPath("paint.csv"));
            //fileWriter=new StreamWriter(File.OpenWrite("paint.csv"));
            //fileWriter.WriteLine("X;Y;Z");
        }

        private void start()
        {
            //socket = new CometdSocket("ws://cefoot.de/socketBtn");
            socket = new CometdSocket("ws://" + Properties.Settings.Default.HOST + ":8080/socketBtn");
            socket.Subscribe("/paint/", PaintHandler);
            socket.Subscribe("/clear/", ClearHandler);

            initKinectSensor();
            registerEventListener();
        }

        private void initKinectSensor()
        {
            this.kinect = KinectSensor.GetDefault();
        }

        private void registerEventListener()
        {
            this.kinect.BodyFrameSource.OpenReader().FrameArrived += Sensor_SkeletonFrameReady;
            //this.kinect.ColorFrameSource.FrameCaptured += Sensor_ColorFrameReady;
            //this.kinect.DepthFrameSource.FrameCaptured += Sensor_DepthFrameReady;


            skeletons = new Body[this.kinect.BodyFrameSource.BodyCount];
            this.kinect.Open();
        }

        //void Sensor_ColorFrameReady(object sender, ColorFrameArrivedEventArgs e)
        //{
        //    imageFrame = e.FrameReference.AcquireFrame();
        //}

        //void Sensor_DepthFrameReady(object sender, DepthFrameArrivedEventArgs e)
        //{
        //    depthFrame = e.FrameReference.AcquireFrame();
        //}

        void Sensor_SkeletonFrameReady(object sender, BodyFrameArrivedEventArgs e)
        {

            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    frame.GetAndRefreshBodyData(skeletons);


                    var skeleton = skeletons.Where(SkeletTracked).FirstOrDefault();

                    if (skeleton != null)
                    {
                        
                        bool rightHandTracked = skeleton.Joints[JointType.HandTipRight].TrackingState == TrackingState.Tracked;
                        CameraSpacePoint currentHandPosition = new CameraSpacePoint();
                        if (rightHandTracked)
                        {
                            currentHandPosition = handPositionBuffer.add(skeleton.Joints[JointType.HandTipRight].Position, 100f);
                        }


                        if (isRecording)
                        {

                            if (handPositionBuffer.IsBufferReady() && rightHandTracked)
                            {
                                this.SendPositionToChannel(currentHandPosition, this.drawPositionChannel);
                                Console.Write(".");
                                allPoints.Add(currentHandPosition);
                            }

                        }
                        else
                        {
                            var result = new CameraSpacePoint();
                            if (getClosePoint(currentHandPosition, out result) && rightHandTracked)
                            {
                                SendPositionToChannel(result, touchChannel);
                                Console.Write(":");
                            }
                            if (rightHandTracked)
                            {
                                var currentPoint = new CameraSpacePoint();
                                currentPoint.X = skeleton.Joints[JointType.HandRight].Position.X * 100;
                                currentPoint.Y = skeleton.Joints[JointType.HandRight].Position.Y * 100;
                                currentPoint.Z = skeleton.Joints[JointType.HandRight].Position.Z * 100;
                                this.SendPositionToChannel(currentPoint, this.handPositionChannel);
                            }

                        }
                        bool viewTracked = skeleton.Joints[JointType.HandLeft].TrackingState == TrackingState.Tracked || skeleton.Joints[JointType.HandLeft].TrackingState == TrackingState.Inferred;
                        viewTracked &= skeleton.Joints[JointType.Head].TrackingState == TrackingState.Tracked || skeleton.Joints[JointType.Head].TrackingState == TrackingState.Inferred;
                        if (viewTracked)
                        {
                            var currentHandLeftPosition = eyePositionBuffer.add(skeleton.Joints[JointType.HandLeft].Position, 100f);
                            SendPositionToChannel(currentHandLeftPosition, eyePositionChannel);
                            var lookAt = lookAtBuffer.add(skeleton.Joints[JointType.Head].Position, 100f);

                            lookAt.X = (2 * currentHandLeftPosition.X - lookAt.X);
                            lookAt.Y = (2 * currentHandLeftPosition.Y - lookAt.Y);
                            lookAt.Z = (2 * currentHandLeftPosition.Z - lookAt.Z);

                            SendPositionToChannel(lookAt, lookAtChannel);
                            Console.Write(",");
                        }
                    }
                }
            }
        }

        private bool SkeletTracked(Body skelet)
        {
            var tracked = skelet.IsTracked;
            return tracked;
        }

        private bool getClosePoint(CameraSpacePoint currentHandPosition, out CameraSpacePoint result)
        {
            double maxDistance = 200;
            bool found = false;
            result = new CameraSpacePoint();
            foreach (CameraSpacePoint point in allPoints)
            {
                var distance = PointDiffSquared(point, currentHandPosition);
                if (distance < maxDistance)
                {
                    result.X = point.X;
                    result.Y = point.Y;
                    result.Z = point.Z;
                    maxDistance = distance;
                    found = true;
                }
            }
            return found;
        }

        private static double PointDiffSquared(CameraSpacePoint a, CameraSpacePoint b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) + (a.Z - b.Z) * (a.Z - b.Z);
        }

        private void SendPositionToChannel(CameraSpacePoint skeletonPoint, string channelName)
        {
            var handPosition = new JsonObject();
            handPosition["X"] = (int)skeletonPoint.X;
            handPosition["Y"] = (int)skeletonPoint.Y;
            handPosition["Z"] = (int)skeletonPoint.Z;
            try
            {
                socket.Send(channelName, handPosition);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private void saveToFile(CameraSpacePoint skeletonPoint)
        {
            fileWriter.WriteLine("{0};{1};{2}", skeletonPoint.X, skeletonPoint.Y, skeletonPoint.Z);
        }

        //private void SendDepthImage()
        //{
        //    if (this.depthFrame != null && this.imageFrame != null)
        //    {
        //        DepthImagePoint[] depthPointData = new DepthImagePoint[depthFrame.Height * depthFrame.Width];
        //        kinect.CoordinateMapper.MapColorFrameToDepthFrame(imageFrame.Format, depthFrame.Format, depthFrame.GetRawPixelData(), depthPointData);
        //        ColorImagePoint[] imagePointData = new ColorImagePoint[imageFrame.Height * imageFrame.Width];
        //        kinect.CoordinateMapper.MapDepthFrameToColorFrame(depthFrame.Format, depthFrame.GetRawPixelData(), imageFrame.Format, imagePointData);
        //    }
        //}

        //JsonObject ParseDepthImagePointArrayToJsonObject(DepthImagePoint[] depthPointArray, ColorImageFrame colorImageFrame)
        //{
        //    JsonObject simpleObject = new JsonObject();

        //    foreach (DepthImagePoint singlePoint in depthPointArray)
        //    {

        //    }

        //    return simpleObject;
        //}

        private void PaintHandler(JsonObject msg)
        {
            var data = msg.ToString();
            if (msg.ContainsKey("data"))
            {
                data = msg["data"].ToString();
            }
            switch (data)
            {
                case "start":
                    StartRecognize();
                    break;
                case "stop":
                    StopRecognize();
                    break;
            }
        }

        private void ClearHandler(JsonObject msg)
        {
            var data = msg.ToString();
            if (msg.ContainsKey("data"))
            {
                data = msg["data"].ToString();
            }
            switch (data)
            {
                case "clear":
                    allPoints.Clear();
                    break;
                case "stop":
                    StopRecognize();
                    break;
            }
        }

        private void StopRecognize()
        {
            Console.WriteLine("StopRecognize");
            this.isRecording = false;
        }

        private void StartRecognize()
        {
            Console.WriteLine("StartRecognize");
            this.isRecording = true;
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (kinect != null && kinect.IsOpen)
            {
                kinect.Close();
            }
            if (fileWriter != null)
            {
                fileWriter.Dispose();
            }
        }
    }
}
