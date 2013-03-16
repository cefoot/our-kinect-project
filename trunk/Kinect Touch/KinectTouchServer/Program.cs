using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace KinectTouchServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var kinectSensorCollection = KinectSensor.KinectSensors;
            var kinectSensor = kinectSensorCollection[0];
            var key = new ConsoleKeyInfo('S', ConsoleKey.S, false, false, false);
            StartKinect(kinectSensor);
            do
            {
                
                Running = true;
                //ThreadPool.QueueUserWorkItem(WorkOnKinect, kinectSensor);
            } while ((key = Console.ReadKey()).Key != ConsoleKey.X);
            StopKinect(kinectSensor);
        }

        protected static bool Running { get; set; }

        private static void StopKinect(KinectSensor kinectSensor)
        {
            Console.WriteLine("stopping..");
            Running = false;
            kinectSensor.Stop();
        }

        private static void StartKinect(KinectSensor kinectSensor)
        {
            Console.WriteLine("starting...");
            kinectSensor.Start();
            Console.WriteLine(Properties.Settings.Default.ImageFormat);
            kinectSensor.DepthStream.Enable(Properties.Settings.Default.ImageFormat);
            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
            var colorImageFrame = kinectSensor.ColorStream.OpenNextFrame(5000);
            
            //var imageToBitmap = ImageToBitmap(colorImageFrame);
            //imageToBitmap.Save(@"D:\work\image.jpg",ImageFormat.Jpeg);
            kinectSensor.ColorStream.Disable();
            Console.WriteLine("started");
        }
        static Bitmap ImageToBitmap(ColorImageFrame image)
        {
            var pixeldata = new byte[image.PixelDataLength];
            image.CopyPixelDataTo(pixeldata);
            var bmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppRgb);
            var bmapdata = bmap.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.WriteOnly,
                bmap.PixelFormat);
            var ptr = bmapdata.Scan0;
            Marshal.Copy(pixeldata, 0, ptr, image.PixelDataLength);
            bmap.UnlockBits(bmapdata);
            return bmap;
        }
    }
}
