using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        double minDiff = 50;
        SkeletonPoint lastPosition = new SkeletonPoint();
        String handPositionChannel = "/datachannel/handPosition";


        static void Main(string[] args)
        {
            using (Program pro = new Program())
            {
                pro.start();
                Console.ReadLine();
            }
        }

        public Program()
        {
            //Console.WriteLine(Path.GetFullPath("paint.csv"));
            //fileWriter=new StreamWriter(File.OpenWrite("paint.csv"));
            //fileWriter.WriteLine("X;Y;Z");
        }

        private void start()
        {
            socket = new CometdSocket("ws://localhost:8080/socketBtn");
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
            this.kinect.DepthFrameReady +=new EventHandler<DepthImageFrameReadyEventArgs>(Sensor_DepthFrameReady);
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
                    
                    if (isRecording)
                    {
                        if (skeleton != null)
                        {
                            //saveToFile(skeleton.Joints[JointType.HandRight].Position);
                            //send hand position to webserver
                            var currentHandPosition=skeleton.Joints[JointType.HandRight].Position;
                            if (PointDiffSquared(currentHandPosition,lastPosition)>this.minDiff)
                            {
                                this.sendHandPosition(skeleton.Joints[JointType.HandRight].Position);
                                lastPosition = currentHandPosition;
                                Console.Write(".");
                            }
                            //send correct image+depthmapping to webserver
                            //this.SendDepthImage();
                            
                        }
                        else
                        {
                            Console.WriteLine("want to record but no skeleton...");
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        private static double PointDiffSquared(SkeletonPoint a, SkeletonPoint b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) + (a.Z - b.Z) * (a.Z - b.Z);
        }

        private void sendHandPosition(SkeletonPoint skeletonPoint)
        {
            JsonObject handPosition = new JsonObject();
            handPosition["X"] = skeletonPoint.X;
            handPosition["Y"] = skeletonPoint.Y;
            handPosition["Z"] = skeletonPoint.Z;
            socket.send(this.handPositionChannel, handPosition);
 
        }

        private void saveToFile(SkeletonPoint skeletonPoint)
        {
            fileWriter.WriteLine("{0};{1};{2}", skeletonPoint.X, skeletonPoint.Y, skeletonPoint.Z);
        }

        private void SendDepthImage()
        {
            if (this.depthFrame != null && this.imageFrame != null)
            {
               DepthImagePoint[] depthPointData = new DepthImagePoint[depthFrame.Height*depthFrame.Width];
               kinect.CoordinateMapper.MapColorFrameToDepthFrame(imageFrame.Format, depthFrame.Format, depthFrame.GetRawPixelData(), depthPointData);
               ColorImagePoint[] imagePointData = new ColorImagePoint[imageFrame.Height * imageFrame.Width];
               kinect.CoordinateMapper.MapDepthFrameToColorFrame(depthFrame.Format, depthFrame.GetRawPixelData(), imageFrame.Format, imagePointData);
            }
        }

        JsonObject ParseDepthImagePointArrayToJsonObject(DepthImagePoint[] depthPointArray, ColorImageFrame colorImageFrame)
        {
            JsonObject simpleObject = new JsonObject();
            
            foreach(DepthImagePoint singlePoint in depthPointArray)
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
