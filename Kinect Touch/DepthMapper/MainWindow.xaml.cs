using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.DepthBasics;
using Microsoft.Xna.Framework;
using Panel = System.Windows.Controls.Panel;
using Vector4 = Microsoft.Kinect.Vector4;

namespace DepthMapper
{

    public enum Mode
    {
        Starting = 0,
        UpperLeft = 1,
        UpperRight = 2,
        LowerRight = 3,
        LowerLeft = 4,
        Running = 5,
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor;
        private WriteableBitmap _bitmap;
        private byte[] _bitmapBits;
        private ColorImagePoint[] _mappedDepthLocations;
        private byte[] _colorPixels = new byte[0];
        private DepthImagePixel[] _depthPixels = new DepthImagePixel[0];
        private DepthImagePixel[] _backgroundDepth = new DepthImagePixel[0];
        private Vector4[] _currentDepthPixel = new Vector4[0];
        private readonly object _currentDepthPixelLockObj = new object();
        private int _bitmapPixelWidth;
        private int _bitmapPixelHeight;

        private void SetSensor(KinectSensor newSensor)
        {
            if (_sensor != null)
            {
                _sensor.Stop();
            }

            _sensor = newSensor;

            if (_sensor == null) return;
            Debug.Assert(_sensor.Status == KinectStatus.Connected, "This should only be called with Connected sensors.");
            _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            _sensor.AllFramesReady += SensorAllFramesReady;
            _sensor.Start();
            ThreadPool.QueueUserWorkItem(WorkOnBackGroundImage);
        }

        void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            var gotColor = false;
            var gotDepth = false;

            using (var colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    Debug.Assert(colorFrame.Width == 640 && colorFrame.Height == 480, "This app only uses 640x480.");

                    if (_colorPixels.Length != colorFrame.PixelDataLength)
                    {
                        _colorPixels = new byte[colorFrame.PixelDataLength];
                        _bitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);
                        _bitmapPixelWidth = _bitmap.PixelWidth;
                        _bitmapPixelHeight = _bitmap.PixelHeight;
                        _bitmapBits = new byte[640 * 480 * 4];
                        Image.Source = _bitmap;
                    }

                    colorFrame.CopyPixelDataTo(_colorPixels);
                    gotColor = true;
                }
            }

            using (var depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    Debug.Assert(depthFrame.Width == 640 && depthFrame.Height == 480, "This app only uses 640x480.");

                    if (_depthPixels.Length != depthFrame.PixelDataLength)
                    {
                        _depthPixels = new DepthImagePixel[depthFrame.PixelDataLength];
                        _mappedDepthLocations = new ColorImagePoint[depthFrame.PixelDataLength];
                    }

                    depthFrame.CopyDepthImagePixelDataTo(_depthPixels);

                    if (BackgroundImgCnt < 100)
                    {
                        BackgroundImgCnt++;
                        _backgroundDepth = new DepthImagePixel[depthFrame.PixelDataLength];
                        depthFrame.CopyDepthImagePixelDataTo(_backgroundDepth);
                        //Configuration.addToBackground(_backgroundDepth);
                    }
                    else if (BackgroundImgCnt == 100){
                        BackgroundImgCnt++;
                        ChangeMode(Mode.UpperLeft);
                    }
                    //{
                    //    BackgroundImgCnt++;
                    //    Configuration.create3DBackgorund(_sensor.CoordinateMapper, DepthImageFormat.Resolution640x480Fps30);
                    //    Configuration.calculatePlanes();
                    //}
                    gotDepth = true;
                }
            }
            if (!gotDepth || !gotColor) return;

            // Put the color image into _bitmapBits
            for (var i = 0; i < _colorPixels.Length; i += 4)
            {
                _bitmapBits[i + 3] = 255;
                _bitmapBits[i + 2] = _colorPixels[i + 2];
                _bitmapBits[i + 1] = _colorPixels[i + 1];
                _bitmapBits[i] = _colorPixels[i];
            }

            _sensor.CoordinateMapper.MapDepthFrameToColorFrame(DepthImageFormat.Resolution640x480Fps30, _depthPixels, ColorImageFormat.RgbResolution640x480Fps30, _mappedDepthLocations);
            var foundpoints = new List<Vector4>();
            for (var i = _depthPixels.Length - 1; i >= 0; i--)
            {
                var depthImagePixel = _depthPixels[i];
                var backgroundDepthImagePixel = _backgroundDepth[i];
                if (!depthImagePixel.IsKnownDepth) continue;
                if (!backgroundDepthImagePixel.IsKnownDepth) continue;
                var depthVal = depthImagePixel.Depth;

                if (depthVal <= 400) continue;//näher macht kein sinn kinect erkennt hier nix mehr
                var distance = Math.Abs(depthVal - backgroundDepthImagePixel.Depth);//unterschied zum hintergrund
                if (distance < 40) continue;//unterschied mehr als x mm
                if (distance > 200) continue;//unterschied weniger als x mm
                var point = _mappedDepthLocations[i];

                if ((point.X < 0 || point.X >= 640) || (point.Y < 0 || point.Y >= 480)) continue;
                foundpoints.Add(new Vector4 { X = point.X, Y = point.Y, Z = depthVal, W = distance });
            }
            //_bitmap.WritePixels(new Int32Rect(0, 0, _bitmapPixelWidth, _bitmapPixelHeight), _bitmapBits, _bitmapPixelWidth * sizeof(int), 0);
            Monitor.TryEnter(_currentDepthPixelLockObj, 200);
            _currentDepthPixel = foundpoints.ToArray();
            Monitor.Exit(_currentDepthPixelLockObj);
        }

        public delegate void ChangeModeDelegate(Mode mode);
        private void ChangeMode(Mode mode)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new ChangeModeDelegate(ChangeMode), DispatcherPriority.Normal, mode);
                return;
            }
            Debug.WriteLine("changing mode to:" + mode);
            Mode = mode;
            switch (mode)
            {
                case Mode.UpperLeft:
                    Panel.SetZIndex(upperLeft, 3);
                    Panel.SetZIndex(upperRight, 1);
                    Panel.SetZIndex(lowerRight, 1);
                    Panel.SetZIndex(lowerLeft, 1);
                    break;
                case Mode.UpperRight:
                    Panel.SetZIndex(upperLeft, 1);
                    Panel.SetZIndex(upperRight, 3);
                    Panel.SetZIndex(lowerRight, 1);
                    Panel.SetZIndex(lowerLeft, 1);
                    break;
                case Mode.LowerRight:
                    Panel.SetZIndex(upperLeft, 1);
                    Panel.SetZIndex(upperRight, 1);
                    Panel.SetZIndex(lowerRight, 3);
                    Panel.SetZIndex(lowerLeft, 1);
                    break;
                case Mode.LowerLeft:
                    Panel.SetZIndex(upperLeft, 1);
                    Panel.SetZIndex(upperRight, 1);
                    Panel.SetZIndex(lowerRight, 1);
                    Panel.SetZIndex(lowerLeft, 3);
                    break;
                default:
                    Panel.SetZIndex(upperLeft, 1);
                    Panel.SetZIndex(upperRight, 1);
                    Panel.SetZIndex(lowerRight, 1);
                    Panel.SetZIndex(lowerLeft, 1);
                    break;
            }
        }

        protected Mode Mode { get; set; }

        protected int BackgroundImgCnt { get; set; }

        public delegate void WritePixels(Int32Rect sourceRect, Array pixels, int stride, int offset);
        public void WorkOnBackGroundImage(Object state)
        {
            if (_currentDepthPixel.Length <= 0)
            {
                Thread.Sleep(500);
                ThreadPool.QueueUserWorkItem(WorkOnBackGroundImage);
                return;
            }
            try
            {
                Monitor.TryEnter(_currentDepthPixelLockObj, 200);
                var data = new Vector4[_currentDepthPixel.Length];
                _currentDepthPixel.CopyTo(data, 0);
                Monitor.Exit(_currentDepthPixelLockObj);
                FindPointCloud(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            ThreadPool.QueueUserWorkItem(WorkOnBackGroundImage);
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            Leftdown = 0x00000002,
            Leftup = 0x00000004,
            Middledown = 0x00000020,
            Middleup = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            Rightdown = 0x00000008,
            Rightup = 0x00000010
        }

        private void FindPointCloud(ICollection<Vector4> data)
        {
            List<Vector4> pointCloud;
            var dismissed = new List<Vector4>();
            do
            {
                var lowestPoint = data.FirstOrDefault();
                foreach (var vector in data)
                {
                    if (vector.Y > lowestPoint.Y && !dismissed.Contains(vector))
                    {
                        lowestPoint = vector;
                    }
                }
                dismissed.Add(lowestPoint);
                pointCloud = data.Where(vec => Vector3.Distance(lowestPoint.ToVector3(), vec.ToVector3()) < 30).ToList();
            } while (pointCloud.Count < 30 && dismissed.Count < data.Count);
            if (pointCloud.Count < 30) return;
            decimal x = 0,
                    y = 0,
                    z = 0,
                    w = 0;

            foreach (var vector in pointCloud)
            {
                x += (decimal)vector.X;
                y += (decimal)vector.Y;
                z += (decimal)vector.Z;
                w += (decimal)vector.W;
            }
            var middlPoint = new Vector4
                                 {
                                     X = (float)(x / pointCloud.Count),
                                     Y = (float)(y / pointCloud.Count),
                                     Z = (float)(z / pointCloud.Count),
                                     W = (float)(w / pointCloud.Count)
                                 };
            WorkOnMiddlPoint(pointCloud, middlPoint);
        }

        private void WorkOnMiddlPoint(List<Vector4> pointCloud, Vector4 middlPoint)
        {
            switch (Mode)
            {
                case Mode.LowerLeft:
                case Mode.UpperLeft:
                case Mode.UpperRight:
                case Mode.LowerRight:
                    WorkOnMiddlPointCalibrate(pointCloud, middlPoint);
                    break;
                case Mode.Running:
                    WorkOnMiddlPointRunning(pointCloud, middlPoint);
                    break;
                default:
                    DrawImage();
                    break;
            }
        }

        private void WorkOnMiddlPointCalibrate(List<Vector4> pointCloud, Vector4 middlPoint)
        {
            if(pointCloud.Count < 30) return;
            switch (Mode)
            {
                case Mode.LowerLeft:
                    if (Vector3.Distance(LowerRightPoint.ToVector3(), middlPoint.ToVector3()) < 100) return;
                    LowerLeftPoint = middlPoint;
                    break;
                case Mode.UpperLeft:
                    UpperLeftPoint = middlPoint;
                    break;
                case Mode.UpperRight:
                    if (Vector3.Distance(UpperLeftPoint.ToVector3(), middlPoint.ToVector3()) < 100) return;
                    UpperRightPoint = middlPoint;
                    break;
                case Mode.LowerRight:
                    if (Vector3.Distance(UpperRightPoint.ToVector3(), middlPoint.ToVector3()) < 100) return;
                    LowerRightPoint = middlPoint;
                    break;
            }
            ChangeMode(Mode + 1);
        }

        protected Vector4 LowerRightPoint { get; set; }

        protected Vector4 UpperRightPoint { get; set; }

        protected Vector4 UpperLeftPoint { get; set; }

        protected Vector4 LowerLeftPoint { get; set; }

        private void WorkOnMiddlPointRunning(IEnumerable<Vector4> pointCloud, Vector4 middlPoint)
        {
            var mouseX = (int)((middlPoint.X - UpperLeftPoint.X) / (LowerRightPoint.X - UpperLeftPoint.X) * Screen.PrimaryScreen.Bounds.Width);
            var mouseY = (int)((middlPoint.Z - UpperLeftPoint.Z) / (LowerRightPoint.Z - UpperLeftPoint.Z) * Screen.PrimaryScreen.Bounds.Height);
            var changed = false;
            if (middlPoint.W < 70)
            {
                mouse_event((int)MouseEventFlags.Leftdown, mouseX, mouseY, 0, 0);
                mouse_event((int)MouseEventFlags.Leftup, mouseX, mouseY, 0, 0);
                foreach (var baseIndex in pointCloud.Select(point => (int)(point.Y * 640 + point.X) * 4))
                {
                    changed = true;
                    _bitmapBits[baseIndex] = 255;
                    _bitmapBits[baseIndex + 1] = 0;
                    _bitmapBits[baseIndex + 2] = 0;
                }
            }else if (middlPoint.W > 80)
            {
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(mouseX, mouseY);
                foreach (var baseIndex in pointCloud.Select(point => (int)(point.Y * 640 + point.X) * 4))
                {
                    changed = true;
                    _bitmapBits[baseIndex] = (byte)((_bitmapBits[baseIndex] + 255) >> 1);
                    _bitmapBits[baseIndex + 1] = (byte)((_bitmapBits[baseIndex + 1] + 255) >> 1);
                    _bitmapBits[baseIndex + 2] = (byte)((_bitmapBits[baseIndex + 2] + 255) >> 1);
                }
            }
            if (changed)
            {
                DrawImage();
            }
        }

        private void DrawImage()
        {
            if (_bitmap.Dispatcher.CheckAccess())
            {
                _bitmap.WritePixels(new Int32Rect(0, 0, _bitmapPixelWidth, _bitmapPixelHeight), _bitmapBits, _bitmapPixelWidth * sizeof(int), 0);
            }
            else
            {
                _bitmap.Dispatcher.Invoke(new WritePixels(_bitmap.WritePixels), DispatcherPriority.Send, new Int32Rect(0, 0, _bitmapPixelWidth, _bitmapPixelHeight), _bitmapBits, _bitmapPixelWidth * sizeof(int), 0);
            }
        }

        public static Vector3 ToVector3(SkeletonPoint point)
        {
            return new Vector3(
                point.X, point.Y, point.Z);
        }

        public MainWindow()
        {
            InitializeComponent();
            ChangeMode(Mode.Starting);
            Configuration = new Configuration(640, 480);

            KinectSensor.KinectSensors.StatusChanged += (object sender, StatusChangedEventArgs e) =>
            {
                if (e.Sensor == _sensor)
                {
                    if (e.Status != KinectStatus.Connected)
                    {
                        SetSensor(null);
                    }
                }
                else if ((_sensor == null) && (e.Status == KinectStatus.Connected))
                {
                    SetSensor(e.Sensor);
                }
            };

            foreach (var sensor in KinectSensor.KinectSensors)
            {
                if (sensor.Status == KinectStatus.Connected)
                {
                    SetSensor(sensor);
                }
            }
        }

        public Configuration Configuration { get; set; }
    }
}

