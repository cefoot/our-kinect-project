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
        double minDiff = 0.5;
        SkeletonPoint lastPosition = new SkeletonPoint();
        String handPositionChannel = "/datachannel/handPosition";
        String headPositionChannel = "/datachannel/headPosition";



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
                            if (PointDiffSquared(currentHandPosition, lastPosition) > this.minDiff)
                            {
                                this.sendHandPosition(currentHandPosition);
                                lastPosition = currentHandPosition;
                                Console.Write(".");
                            }
                            else
                            {
                                Console.Write("-");
                            }


                            //send correct image+depthmapping to webserver
                            //this.SendDepthImage();
                            
                        }
                        else
                        {
                            Console.WriteLine("want to record HAND but no skeleton...");
                        }
                    }
                    else
                    {
                        if (skeleton != null)
                        {
                            var currentHeadPosition = skeleton.Joints[JointType.Head].Position;
                            this.sendHeadPosition(skeleton.Joints[JointType.HandRight].Position);
                        }
                        else
                        {
                            Console.WriteLine("want to record HEAD but no skeleton...");
                        }
                        
                    }
                }
            }
        }

        private void sendHeadPosition(SkeletonPoint skeletonPoint)
        {
            JsonObject headPosition = new JsonObject();
            headPosition["X"] = (int) skeletonPoint.X;
            headPosition["Y"] = (int) skeletonPoint.Y;
            headPosition["Z"] = (int) skeletonPoint.Z;
            socket.send(this.headPositionChannel, headPosition);
        }

        private static double PointDiffSquared(SkeletonPoint a, SkeletonPoint b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) + (a.Z - b.Z) * (a.Z - b.Z);
        }

        private void sendHandPosition(SkeletonPoint skeletonPoint)
        {
            JsonObject handPosition = new JsonObject();
            handPosition["X"] = (int) skeletonPoint.X;
            handPosition["Y"] = (int) skeletonPoint.Y;
            handPosition["Z"] = (int) skeletonPoint.Z;
            try
            {
                socket.Send(this.handPositionChannel, handPosition);
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
            Console.WriteLine(msg);
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
