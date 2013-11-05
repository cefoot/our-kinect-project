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
            fileWriter=new StreamWriter(File.OpenWrite("paint.csv"));
            fileWriter.WriteLine("X;Y;Z");
        }

        private void start()
        {
            CometdSocket socket = new CometdSocket("ws://localhost:8080/socketBtn");
            socket.Subscribe("/paint/", PaintHandler);
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
            this.kinect.SkeletonStream.Enable();
            this.kinect.Start();
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
                            saveToFile(skeleton.Joints[JointType.HandRight].Position);
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        private void saveToFile(SkeletonPoint skeletonPoint)
        {
            fileWriter.WriteLine("{0};{1};{2}", skeletonPoint.X, skeletonPoint.Y, skeletonPoint.Z);
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
            this.isRecording = true;
        }

        private void StartRecognize()
        {

            this.isRecording = false;
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
