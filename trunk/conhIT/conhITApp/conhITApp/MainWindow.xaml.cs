using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using De.DataExperts.conhITApp.Properties;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using WpfAnimatedGif;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Size = System.Windows.Size;
using System.Windows.Controls;

namespace De.DataExperts.conhITApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FrameDescription imageFormat;
        KinectSensor kinect;
        private readonly Dictionary<ulong, SkeletData> _skeletData = new Dictionary<ulong, SkeletData>();
        private Dictionary<ulong, Image> _imgs = new Dictionary<ulong, Image>();
        private BitmapSource _face;
        private WriteableBitmap colorBitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitKinectSensor();
            var bitmap = Properties.Resources.face;
            bitmap.MakeTransparent(Color.Blue);
            _face = CreateBitmapSourceFromBitmap(bitmap);
        }

        private void InitKinectSensor()
        {
            this.kinect = KinectSensor.GetDefault();
            if (this.kinect == null) return;

            this.kinect.BodyFrameSource.OpenReader().FrameArrived += Sensor_SkeletonFrameReady;
            this.kinect.ColorFrameSource.OpenReader().FrameArrived += Sensor_ColorFrameReady;
            this.kinect.Open();
            imageFormat = this.kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.colorBitmap = new WriteableBitmap(imageFormat.Width, imageFormat.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        void Sensor_ColorFrameReady(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var image = e.FrameReference.AcquireFrame())
            {
                if (image == null) return;

                FrameDescription colorFrameDescription = image.FrameDescription;
                using (KinectBuffer colorBuffer = image.LockRawImageBuffer())
                {
                    this.colorBitmap.Lock();

                    // verify data and write the new color frame data to the display bitmap
                    if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                    {
                        image.CopyConvertedFrameDataToIntPtr(
                            this.colorBitmap.BackBuffer,
                            (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                            ColorImageFormat.Bgra);

                        this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                    }

                    this.colorBitmap.Unlock();
                }
                
                var myDrawingGroup = new DrawingGroup();
                myDrawingGroup.Children.Add(new ImageDrawing(this.colorBitmap, new Rect(new Size(this.colorBitmap.Width, this.colorBitmap.Height))));
                //myDrawingGroup.Children.Add(new ImageDrawing(data.ToBitmapSource(image.Width, image.Height), new Rect(new Size(image.Width, image.Height))));

                //DrawFaces(myDrawingGroup);//draw smilies

                image1.Source = new DrawingImage(myDrawingGroup);
            }

        }

        private void DrawFaces(DrawingGroup myDrawingGroup)
        {
            foreach (var skeletData in _skeletData.Values)
            {
                var calcWidth = 50d / (skeletData.HeadDistance / 3d);
                var calcHeight = 52.5d / (skeletData.HeadDistance / 3d);

                myDrawingGroup.Children.Add(new ImageDrawing(_face, new Rect(skeletData.HeadPosition.X - ((int)calcWidth / 2), skeletData.HeadPosition.Y - ((int)calcHeight / 2), calcWidth, calcHeight)));

                //myDrawingGroup.Children.Add(new ImageDrawing(c. Properties.Resources.face, new Rect(40, 0, 45, 130)));
            }
        }

        void Sensor_SkeletonFrameReady(object sender, BodyFrameArrivedEventArgs e)
        {

            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var skeletons = new Body[frame.BodyCount];

                    frame.GetAndRefreshBodyData(skeletons);
                    _skeletData.Clear();
                    foreach (var skeleton in skeletons.Where(SkeletTracked))
                    {
                        var skeletDataObject = new SkeletData();
                        if (skeleton != null)
                        {
                            var spine = skeleton.Joints[JointType.SpineMid];
                            var shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
                            var shoulderRight = skeleton.Joints[JointType.ShoulderRight];
                            var head = skeleton.Joints[JointType.Head];
                            if (IsTrackedOrInferred(spine) && IsTrackedOrInferred(shoulderLeft) && IsTrackedOrInferred(shoulderRight))
                            {
                                var spineImagePos = kinect.CoordinateMapper.MapCameraPointToColorSpace(spine.Position);//, imageFormat);
                                var shoulderRightImagePos = kinect.CoordinateMapper.MapCameraPointToColorSpace(shoulderRight.Position);//, imageFormat);
                                var shoulderLeftImagePos = kinect.CoordinateMapper.MapCameraPointToColorSpace(shoulderLeft.Position);//, imageFormat);
                                var heartPos = new ColorSpacePoint
                                                   {
                                                       X = (spineImagePos.X + shoulderLeftImagePos.X) / 2,
                                                       Y = (spineImagePos.Y + shoulderLeftImagePos.Y) / 2
                                                   };
                                skeletDataObject.HeartPosition = heartPos;
                                skeletDataObject.HeartDistance = spine.Position.Z;
                                skeletDataObject.heartWidth = Math.Abs(shoulderRightImagePos.X - shoulderLeftImagePos.X) * 0.6f;
                                skeletDataObject.HeadPosition = kinect.CoordinateMapper.MapCameraPointToColorSpace(head.Position);//, imageFormat);
                                skeletDataObject.HeadDistance = head.Position.Z;

                                _skeletData[skeleton.TrackingId] = skeletDataObject;

                            }
                        }
                    }
                    CreateHearts();
                }

            }

        }

        private void CreateHearts()
        {
            var oldIDs = new List<ulong>(_imgs.Keys);
            var newIDs = new List<ulong>(_skeletData.Keys);
            foreach (var newID in newIDs)
            {
                if (!_imgs.ContainsKey(newID))
                {
                    CreateNewHeart(newIDs, newID);
                }
            }
            foreach (var heartKey in _imgs.Keys)
            {
                if (!newIDs.Contains(heartKey))
                {
                    _imgs[heartKey].Visibility = Visibility.Hidden;
                }
                else
                {
                    _imgs[heartKey].Visibility = Visibility.Visible;
                    var skeletDataObject = _skeletData[heartKey];
                    var calcWidth = container.ActualWidth * (skeletDataObject.heartWidth / kinect.ColorFrameSource.FrameDescription.Width);
                    calcWidth = Math.Max(calcWidth, 34);
                    var calcHeight = calcWidth;//bild ist genauso hoch wie breit
                    MoveHeart(
                        (float)(image1.ActualWidth * skeletDataObject.HeartPosition.X / kinect.ColorFrameSource.FrameDescription.Width - calcWidth / 2f + image1.Margin.Left),
                        (float)(image1.ActualHeight * skeletDataObject.HeartPosition.Y / kinect.ColorFrameSource.FrameDescription.Height - calcHeight / 2f + image1.Margin.Top),
                        (float)calcWidth,
                        (float)calcHeight,
                        _imgs[heartKey]);

                }
            }

        }

        private void CreateNewHeart(ICollection<ulong> newIDs, ulong newID)
        {
            ulong curKey = ulong.MaxValue;
            foreach (var heartKey in _imgs.Keys.Where(heartKey => !newIDs.Contains(heartKey)))
            {
                curKey = heartKey;
                break;
            }
            if (curKey < ulong.MaxValue)
            {
                _imgs[newID] = _imgs[curKey];
                _imgs.Remove(curKey);
            }
            else
            {
                _imgs[newID] = CreateHeart();
            }
        }

        public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    // You need to specify the image format to fill the stream. 
                    // I'm assuming it is PNG
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var bitmapDecoder = BitmapDecoder.Create(
                        memoryStream,
                        BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.OnLoad);

                    // This will disconnect the stream from the image completely...
                    var writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
                    writable.Freeze();

                    return writable;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private Image CreateHeart()
        {
            var heartImg = new Image();
            var image = new BitmapImage();
            image.BeginInit();
            //http://media.photobucket.com/user/Snaazzy/media/gif.gif.html?filters[term]=heart%20gif&filters[primary]=images&filters[secondary]=videos&sort=1&o=0
            image.UriSource = new Uri(@"pack://application:,,,/Resources/heartCustom.gif", UriKind.RelativeOrAbsolute);
            image.EndInit();
            ImageBehavior.SetAnimatedSource(heartImg, image);
            heartImg.VerticalAlignment = VerticalAlignment.Top;
            heartImg.HorizontalAlignment = HorizontalAlignment.Left;
            container.Children.Add(heartImg);
            return heartImg;
        }

        private void MoveHeart(float x, float y, float height, float width, Image heartImg)
        {
            heartImg.Width = width;
            heartImg.Height = height;
            heartImg.Margin = new Thickness(x, y, 0, 0);
        }

        private bool IsTrackedOrInferred(Joint joint)
        {
            return joint.TrackingState == TrackingState.Tracked || joint.TrackingState == TrackingState.Inferred;
        }

        private bool SkeletTracked(Body skelet)
        {
            var tracked = skelet.IsTracked;
            return tracked;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.H:
                    MessageBox.Show("Folgende Hotkeys sind hinterlegt:\n\tF\tVollbild\n\tH\tHilfe\n\tESC\tSchließen", "Hilfe");
                    break;
                case Key.X:
                    MoveHeart(450f, 50f, 50f, 50f, CreateHeart());
                    break;
                case Key.F:
                    WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
                    break;
                case Key.Escape:
                    Application.Current.Shutdown();
                    break;

            }
        }


    }
}
