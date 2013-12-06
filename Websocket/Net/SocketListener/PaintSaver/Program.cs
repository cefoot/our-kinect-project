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
        ColorImageFrame imageFrame;
        DepthImageFrame depthFrame;
        //minimal change in mm to send a new hand position
        double minDiff = 10;
        SkeletonPoint lastPosition = new SkeletonPoint();
        String handPositionChannel = "/datachannel/handPosition";
        //String headPositionChannel = "/datachannel/headPosition";
        String eyePositionChannel = "/datachannel/eyePosition";
        String lookAtChannel = "/datachannel/lookAt";

        PointBuffer handPositionBuffer = new PointBuffer(4,"handPositionBuffer");
        PointBuffer eyePositionBuffer = new PointBuffer(8, "eyePositionBuffer");
        PointBuffer lookAtBuffer = new PointBuffer(8, "lookAtBuffer");




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
            socket = new CometdSocket("ws://ws201736:8080/socketBtn");
            socket.Subscribe("/paint/", PaintHandler);
            //explizite initialisierung, vielleicht unnötig
            lastPosition.X = 0;
            lastPosition.Y = 0;
            lastPosition.Z = 0;


            initKinectSensor();
            registerEventListener();
        }

        private void initKinectSensor()
        {
            this.kinect = KinectSensor.KinectSensors.Where(x => x.Status == KinectStatus.Connected).FirstOrDefault();
        }

        private void registerEventListener()
        {
            this.kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(Sensor_SkeletonFrameReady);
            this.kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(Sensor_ColorFrameReady);
            this.kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(Sensor_DepthFrameReady);
            this.kinect.SkeletonStream.Enable();
            this.kinect.Start();
        }

        void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            imageFrame = e.OpenColorImageFrame();

        }

        void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            depthFrame = e.OpenDepthImageFrame();

        }

        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {

                    Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];

                    frame.CopySkeletonDataTo(skeletons);


                    var skeleton = skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();

                    if (skeleton != null)
                    {
                        if (isRecording)
                        {
                            //saveToFile(skeleton.Joints[JointType.HandRight].Position);
                            //send hand position to webserver

                            var currentHandPosition = skeleton.Joints[JointType.HandRight].Position;
                            handPositionBuffer.add(currentHandPosition, 100f);
                            if(handPositionBuffer.isBufferReady())
                            {
                                this.SendPositionToChannel(handPositionBuffer.getCurrentPoint(100f), this.handPositionChannel);
                                Console.Write(".");
                            }
                            


                        }
                        else
                        {
                            handPositionBuffer.resetBuffer();
                        }
                        SkeletonPoint currentHandLeftPosition = eyePositionBuffer.add(skeleton.Joints[JointType.HandLeft].Position,100f);
                        SendPositionToChannel(currentHandLeftPosition, eyePositionChannel);
                        var lookAt = lookAtBuffer.add(skeleton.Joints[JointType.Head].Position,100f);

                        lookAt.X = (2 * currentHandLeftPosition.X - lookAt.X);
                        lookAt.Y = (2 * currentHandLeftPosition.Y - lookAt.Y);
                        lookAt.Z = (2 * currentHandLeftPosition.Z - lookAt.Z);
                        
                        /*
                        currentHandLeftPosition.X *= 100f;
                        currentHandLeftPosition.Y *= 100f;
                        currentHandLeftPosition.Z *= 100f;
                        SendPositionToChannel(currentHandLeftPosition, eyePositionChannel);
                         * */
                        SendPositionToChannel(lookAt, lookAtChannel);
                        Console.Write(",");
                    }
                }
            }
        }

        private static double PointDiffSquared(SkeletonPoint a, SkeletonPoint b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) + (a.Z - b.Z) * (a.Z - b.Z);
        }

        private void SendPositionToChannel(SkeletonPoint skeletonPoint, string channelName)
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

        private void saveToFile(SkeletonPoint skeletonPoint)
        {
            fileWriter.WriteLine("{0};{1};{2}", skeletonPoint.X, skeletonPoint.Y, skeletonPoint.Z);
        }

        private void SendDepthImage()
        {
            if (this.depthFrame != null && this.imageFrame != null)
            {
                DepthImagePoint[] depthPointData = new DepthImagePoint[depthFrame.Height * depthFrame.Width];
                kinect.CoordinateMapper.MapColorFrameToDepthFrame(imageFrame.Format, depthFrame.Format, depthFrame.GetRawPixelData(), depthPointData);
                ColorImagePoint[] imagePointData = new ColorImagePoint[imageFrame.Height * imageFrame.Width];
                kinect.CoordinateMapper.MapDepthFrameToColorFrame(depthFrame.Format, depthFrame.GetRawPixelData(), imageFrame.Format, imagePointData);
            }
        }

        JsonObject ParseDepthImagePointArrayToJsonObject(DepthImagePoint[] depthPointArray, ColorImageFrame colorImageFrame)
        {
            JsonObject simpleObject = new JsonObject();

            foreach (DepthImagePoint singlePoint in depthPointArray)
            {

            }

            return simpleObject;
        }

        private void PaintHandler(JsonObject msg)
        {

            switch (msg["data"].ToString())
            {
                case "start":
                    StartRecognize();
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
            fileWriter.Dispose();
        }
    }
}
