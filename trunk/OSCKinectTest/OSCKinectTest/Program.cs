using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Ventuz.OSC;

namespace OSCKinectTest
{
    class Program
    {
        private static KinectSensor _sensor;
        private static UdpWriter _skeletWriter;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleCancelKeyPress;

            _skeletWriter = new UdpWriter("255.255.255.0", 666);
            _sensor = GetSensor();
            if(_sensor != null)
                InitKinect(_sensor);
            Console.ReadKey();
            _sensor.Stop();
        }

        static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _sensor.Stop();
        }

        private static KinectSensor GetSensor()
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                return KinectSensor.KinectSensors[0];
            }
            KinectSensor.KinectSensors.StatusChanged += KinectSensorsStatusChanged;
            Console.WriteLine("Waiting for Kinect");
            return null;
        }

        static void KinectSensorsStatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.Status != KinectStatus.Connected) return;
            _sensor = e.Sensor;
            InitKinect(e.Sensor);
        }

        private static void InitKinect(KinectSensor sensor)
        {
            sensor.SkeletonFrameReady += SensorSkeletonFrameReady;
            sensor.SkeletonStream.Enable();
            sensor.DepthFrameReady += SensorDepthFrameReady;
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            sensor.ColorFrameReady += SensorColorFrameReady;
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            sensor.Start();
            
        }

        static void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            Console.WriteLine("ColorFrame");
        }

        static void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            Console.WriteLine("DepthFrame");
        }

        static void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Console.WriteLine("SkeletonFrame");
            using (var frame = e.OpenSkeletonFrame())
            {
                if(frame.SkeletonArrayLength == 0)
                {
                    Console.WriteLine("Kein Skelet");
                    return;
                }
                var skelets = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skelets);
                var skelet = skelets.FirstOrDefault(cur => cur.TrackingState == SkeletonTrackingState.Tracked);
                if(skelet == null)
                {
                    return;
                }
                Debug.WriteLine(frame.FloorClipPlane);
                var head = skelet.Joints[JointType.Head];
                _skeletWriter.Send(new OscElement("/kinect/skelet/head", head.Position.X, head.Position.Y, head.Position.Z));
            }
        }
    }
}
