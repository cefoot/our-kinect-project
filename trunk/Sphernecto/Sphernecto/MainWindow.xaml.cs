using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sphernecto
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double FloorTolerance = 0.05d;
        private const float FootRange = 0.05f;
        private const int UpdateRate = 100;

        private readonly List<BackgroundWorker> _workers = new List<BackgroundWorker>();
        private BackgroundWorker Worker
        {
            get
            {
                var curWrkr = _workers.FirstOrDefault(wrkr => !wrkr.IsBusy);
                if (curWrkr == null)
                {
                    curWrkr = new BackgroundWorker();
                    curWrkr.RunWorkerCompleted += WorkerProgressed;
                    curWrkr.DoWork += worker_DoWork;
                    _workers.Add(curWrkr);
                    Debug.WriteLine(_workers.Count);
                }
                return curWrkr;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            Sensor = KinectSensor.KinectSensors.FirstOrDefault(sens => sens.Status == KinectStatus.Connected);
            if (Sensor == null || Sensor.Status != KinectStatus.Connected)
            {
                MessageBox.Show("Keine Kinect!");
                Application.Current.Shutdown();
            }
            Application.Current.Exit += CurrentExit;
            InitKinect();
        }

        private void InitKinect()
        {
            Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            Sensor.SkeletonStream.Enable();
            Sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            Sensor.DepthFrameReady += Sensor_DepthFrameReady;
            Sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;
            Sensor.ColorFrameReady += Sensor_ColorFrameReady;
            Sensor.Start();
        }

        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame == null) return;
                AnalyzeSkeletFrame(frame);
            }
        }

        void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null) return;
                AnalyzeDepthFrame(frame);
            }
        }

        Skeleton _curSkelet = null;
        private bool _IsCustomFloor = false;

        private Tuple<float, float, float, float> _floor;
        protected Tuple<float, float, float, float> Floor
        {
            get { return _floor; }
            set
            {
                if (!_IsCustomFloor)
                {
                    _floor = value;
                }
            }
        }

        private void AnalyzeSkeletFrame(SkeletonFrame frame)
        {
            var skelets = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo(skelets);
            _curSkelet = _curSkelet != null ?
                skelets.FirstOrDefault(sk => sk.TrackingId == _curSkelet.TrackingId)
                : skelets.FirstOrDefault(
                    sk => sk.Joints[JointType.FootLeft].TrackingState == JointTrackingState.Tracked
                        || sk.Joints[JointType.FootLeft].TrackingState == JointTrackingState.Inferred);
            Floor = frame.FloorClipPlane;
        }

        private DepthImagePixel[] depthImage = null;

        private DepthImagePixel[] DepthImage { get { return depthImage; } }

        private void AnalyzeDepthFrame(DepthImageFrame frame)
        {
            if (DepthImage == null)
            {
                depthImage = new DepthImagePixel[frame.PixelDataLength];
            }
            frame.CopyDepthImagePixelDataTo(DepthImage);

            Worker.RunWorkerAsync(DepthImage);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var pixels = e.Argument as DepthImagePixel[];
            var skeletPixels = new SkeletonPoint[pixels.Length];
            Sensor.CoordinateMapper.MapDepthFrameToSkeletonFrame(Sensor.DepthStream.Format, pixels, skeletPixels);
            var curFloorPxl = Floor;
            if (Floor == null || (0 == Floor.Item1 && 0 == Floor.Item2 && 0 == Floor.Item3 && 0 == Floor.Item4))
            {
                Debug.WriteLine("No Floor");
                curFloorPxl = new Tuple<float, float, float, float>(0, 0, 0, 5);
            }
            var _drawData = new byte[_imgData.Length];
            for (var i = 0; i < skeletPixels.Length; i++)
            {
                var px = skeletPixels[i];
                var abs = Math.Abs(px.X * curFloorPxl.Item1 + px.Y * curFloorPxl.Item2 + px.Z * curFloorPxl.Item3 + curFloorPxl.Item4);
                var drawIdx = i * 4;
                Joint leftFoot;
                if (_curSkelet != null &&
                    (leftFoot = _curSkelet.Joints[JointType.FootLeft]).TrackingState == JointTrackingState.Tracked
                    && px.X.IsInRange(leftFoot.Position.X, FootRange)
                    && px.Y.IsInRange(leftFoot.Position.Y, FootRange)
                    && px.Z.IsInRange(leftFoot.Position.Z, FootRange))
                {//FOOT!
                    _drawData[drawIdx] = 0;
                    _drawData[drawIdx + 1] = 0;
                    _drawData[drawIdx + 2] = 255;
                    _drawData[drawIdx + 3] = 0;
                }
                else if (abs < FloorTolerance)
                {//Boden
                    _drawData[drawIdx] = 0;
                    _drawData[drawIdx + 1] = 0;
                    _drawData[drawIdx + 2] = 0;
                    _drawData[drawIdx + 3] = 0;
                }
                else
                {//Rest

                    _drawData[drawIdx] = _imgData[drawIdx];
                    _drawData[drawIdx + 1] = _imgData[drawIdx + 1];
                    _drawData[drawIdx + 2] = _imgData[drawIdx + 2];
                    _drawData[drawIdx + 3] = _imgData[drawIdx + 3];
                }
            }
            //Debug.WriteLine("WorkerThread:{0}", Thread.CurrentThread.ManagedThreadId);
            e.Result = _drawData;
        }

        private byte[] _imgData = null;

        void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var image = e.OpenColorImageFrame())
            {
                if (image == null) return;
                var data = new byte[image.PixelDataLength];
                image.CopyPixelDataTo(data);
                _imgData = data;
            }
            //Debug.WriteLine("ColorThread:{0}",Thread.CurrentThread.ManagedThreadId);
        }

        private DateTime lastUpdate = DateTime.Now;

        void WorkerProgressed(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((DateTime.Now - lastUpdate).TotalMilliseconds < UpdateRate) return;//so schnell kommt die oberfläche nicht mit
            if (e.Result == null) return;
            var myDrawingGroup = new DrawingGroup();
            myDrawingGroup.Children.Add(new ImageDrawing((e.Result as byte[]).ToBitmapSource(Sensor.ColorStream.FrameWidth, Sensor.ColorStream.FrameHeight), new Rect(new Size(Sensor.ColorStream.FrameWidth, Sensor.ColorStream.FrameHeight))));

            image1.Source = new DrawingImage(myDrawingGroup);
            lastUpdate = DateTime.Now;
            //Debug.WriteLine("ProgressedThread:{0}", Thread.CurrentThread.ManagedThreadId);
        }

        void CurrentExit(object sender, ExitEventArgs e)
        {
        }

        protected KinectSensor Sensor { get; set; }

        List<SkeletonPoint> ClickedPxl = new List<SkeletonPoint>();

        private void image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(image1);
            var pxl = new DepthImagePoint
                          {
                              X = (int)(pos.X / image1.ActualWidth * Sensor.DepthStream.FrameWidth),
                              Y = (int)(pos.Y / image1.ActualHeight * Sensor.DepthStream.FrameHeight)
                          };
            pxl.Depth = DepthImage[pxl.Y * Sensor.DepthStream.FrameWidth + pxl.X].Depth;
            var pnt = Sensor.CoordinateMapper.MapDepthPointToSkeletonPoint(Sensor.DepthStream.Format, pxl);
            ClickedPxl.Add(pnt);
            Debug.WriteLine("X:{0:0.00}Y:{1:0.00}Z:{2:0.00}", pnt.X, pnt.Y, pnt.Z);
            if (ClickedPxl.Count == 3)
            {
                //calc
                _IsCustomFloor = true;
                _floor = CalcFloor(ClickedPxl);
            }
        }

        private static Tuple<float, float, float, float> CalcFloor(IReadOnlyList<SkeletonPoint> clickedPxl)
        {
            var p1 = clickedPxl[0];
            var p2 = clickedPxl[1];
            var p3 = clickedPxl[2];
            var ax = p1.X - p3.X;
            var ay = p1.Y - p3.Y;
            var az = p1.Z - p3.Z;
            var bx = p2.X - p3.X;
            var by = p2.Y - p3.Y;
            var bz = p2.Z - p3.Z;
            var a = ay * bz - az * by;
            var b = az * bx - ax * bz;
            var c = ax * by - ay * bx;
            var d = a * p3.X + b * p3.Y + c * p3.Z;
            return new Tuple<float, float, float, float>(a, b, c, d);
        }
    }
}
