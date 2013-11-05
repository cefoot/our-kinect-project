using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleJson;
using SocketListener;
using Microsoft.Kinect;

namespace PaintSaver
{
    class Program
    {
        KinectSensor kinect;
        Boolean isRecording = false;
        static void Main(string[] args)
        {
            Program pro = new Program();
            pro.start();
            Console.ReadLine();
          
        }

        private void start()
        {
            CometdSocket socket = new CometdSocket("ws://localhost:8080/socketBtn");
            socket.Subscribe("/paint/", PaintHandler);
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
                    if (isRecording)
                    {
                        
                    }
                    else
                    {
                    }
                }
            }
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
    }
}
