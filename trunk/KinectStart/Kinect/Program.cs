using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Coding4Fun.Kinect.WinForm;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Common;

namespace Kinect
{
    static class Program
    {
        private static Form1 _mainForm;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var kinect = KinectSensor.KinectSensors[0];
            //var nxtFram = kinect.DepthStream.OpenNextFrame(10);
            kinect.Start();// alt Initialize(RuntimeOptions.UseSkeletalTracking);//was will ich haben
            //kinect.ElevationAngle = 5;//neigung
            kinect.SkeletonStream.Enable();
            kinect.SkeletonFrameReady += RuntimeSkeletonFrameReady;
            //blödsinn Handled(kinect);
            //nur damit program offen bleibt
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _mainForm = new Form1();
            Application.Run(_mainForm);
        }

        

        static void RuntimeSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var openSkeletonFrame = e.OpenSkeletonFrame();

            var skeletons = new Skeleton[openSkeletonFrame.SkeletonArrayLength];
            openSkeletonFrame.CopySkeletonDataTo(skeletons);
            var handRight = new Joint();
            foreach (var skeleton in skeletons.Where(skeleton => skeleton != null).Where(skeleton => skeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked))
            {
                handRight = skeleton.Joints[JointType.HandRight];
                break;
            }

            if(handRight.TrackingState == JointTrackingState.Tracked)
            {
                _mainForm.BeginInvoke(new MethodInvoker(() => _mainForm.label1.Text = PosToString(handRight.Position)));
                var point = CalculatePoint(handRight.Position);
                Console.WriteLine(point);
                _mainForm.BeginInvoke(new MethodInvoker(() =>
                                                            {
                                                                _mainForm.Left = point.X - _mainForm.Width/2;
                                                                _mainForm.Top = point.Y - _mainForm.Height/2;
                                                            }));
                //Cursor.Position = new Point(head.Position.X, head.Position.Y);
            }
        }

        private static Point CalculatePoint(Joint handRight)
        {
            var screen = Screen.PrimaryScreen;
            _mainForm.Invoke(new MethodInvoker(() => screen = Screen.FromPoint(_mainForm.Location)));

            var scaleTo = handRight.ScaleTo(screen.Bounds.Width, screen.Bounds.Height);

            return new Point((int)scaleTo.Position.X + screen.Bounds.Left, (int)scaleTo.Position.Y);

        }

        private static Point CalculatePoint(SkeletonPoint position)
        {
            var screen = Screen.PrimaryScreen;
            _mainForm.Invoke(new MethodInvoker(() => screen = Screen.FromPoint(_mainForm.Location)));

            var xVal = position.X + 1.0f;//damit nichtmehr negativ
            var yVal = position.Y * -1 + 1.0f;//umdrehen damit 0 oben
            var posX = ((float)screen.Bounds.Width / 2) * xVal;
            var posY = ((float)screen.Bounds.Height / 2) * yVal;
            return new Point((int) posX + screen.Bounds.Left, (int) posY);
        }

        private static string PosToString(SkeletonPoint position)
        {
            return String.Format("X:{0} Y:{1} Z:{2}", position.X, position.Y, position.Z);
        }
    }
}
