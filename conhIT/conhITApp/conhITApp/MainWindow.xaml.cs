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
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using WpfAnimatedGif;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Size = System.Windows.Size;

namespace conhITApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ColorImageFormat imageFormat = ColorImageFormat.RgbResolution640x480Fps30;
        KinectSensor kinect;
        private readonly Dictionary<int,SkeletData> _skeletData = new Dictionary<int, SkeletData>();
        private Dictionary<int,Image> _imgs = new Dictionary<int, Image>();
        private BitmapSource _face;

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
            this.kinect = KinectSensor.KinectSensors.Where(x => x.Status == KinectStatus.Connected).FirstOrDefault();
            this.kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(Sensor_SkeletonFrameReady);
            this.kinect.SkeletonStream.Enable();
            this.kinect.ColorStream.Enable();
            this.kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(Sensor_ColorFrameReady);
            this.kinect.Start();
        }

        void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var image = e.OpenColorImageFrame())
            {
                if (image == null) return;
                var data = new byte[image.PixelDataLength];
                image.CopyPixelDataTo(data);
                var myDrawingGroup = new DrawingGroup();
                myDrawingGroup.Children.Add(new ImageDrawing(data.ToBitmapSource(image.Width, image.Height), new Rect(new Size(image.Width, image.Height))));

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

                tblHeight.Margin = new Thickness(skeletData.HeadPosition.X, skeletData.HeadPosition.Y, 0, 0);
                //myDrawingGroup.Children.Add(new ImageDrawing(c. Properties.Resources.face, new Rect(40, 0, 45, 130)));
            }
        }

        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    var skeletons = new Skeleton[frame.SkeletonArrayLength];

                    frame.CopySkeletonDataTo(skeletons);
                    _skeletData.Clear();
                    foreach (var skeleton in skeletons.Where(SkeletTracked))
                    {
                        var skeletDataObject = new SkeletData();
                        if (skeleton != null)
                        {
                            Debug.WriteLine(skeleton.TrackingId);
                            var spine = skeleton.Joints[JointType.Spine];
                            var shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
                            var shoulderRight = skeleton.Joints[JointType.ShoulderRight];
                            var head = skeleton.Joints[JointType.Head];
                            if (IsTrackedOrInferred(spine) && IsTrackedOrInferred(shoulderLeft) && IsTrackedOrInferred(shoulderRight))
                            {
                                var spineImagePos = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(spine.Position, imageFormat);
                                var shoulderRightImagePos = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(shoulderRight.Position, imageFormat);
                                var shoulderLeftImagePos = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(shoulderLeft.Position, imageFormat);
                                var heartPos = new ColorImagePoint
                                                   {
                                                       X = (spineImagePos.X + shoulderLeftImagePos.X) / 2,
                                                       Y = (spineImagePos.Y + shoulderLeftImagePos.Y) / 2
                                                   };
                                skeletDataObject.HeartPosition = heartPos;
                                skeletDataObject.HeartDistance = spine.Position.Z;
                                skeletDataObject.heartWidth = Math.Abs(shoulderRightImagePos.X - shoulderLeftImagePos.X) * 0.6f;
                                skeletDataObject.HeadPosition = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(head.Position, imageFormat);
                                skeletDataObject.HeadDistance = head.Position.Z;
                                
                                _skeletData[skeleton.TrackingId]=skeletDataObject;

                            }
                        }
                    }
                    CreateHearts();
                }

            }

        }

        private void CreateHearts()
        {
            var oldIDs = new List<int>(_imgs.Keys);
            var newIDs = new List<int>(_skeletData.Keys);
            foreach (var newID in newIDs)
            {
                if(!_imgs.ContainsKey(newID))
                {
                    CreateNewHeart(newIDs, newID);
                }
            }
            foreach (var heartKey in _imgs.Keys)
            {
                if(!newIDs.Contains(heartKey))
                {
                    _imgs[heartKey].Visibility = Visibility.Hidden;
                }else
                {
                    _imgs[heartKey].Visibility = Visibility.Visible;
                    var skeletDataObject = _skeletData[heartKey];
                    var calcWidth = container.ActualWidth * (skeletDataObject.heartWidth / kinect.ColorStream.FrameWidth);
                    calcWidth = Math.Max(calcWidth, 34);
                    var calcHeight = calcWidth;//bild ist genauso hoch wie breit
                    //Debug.WriteLine(calcHeight);
                    MoveHeart((float)(container.ActualWidth * skeletDataObject.HeartPosition.X / kinect.ColorStream.FrameWidth - calcWidth / 2), (float)(container.ActualHeight * skeletDataObject.HeartPosition.Y / kinect.ColorStream.FrameHeight - calcHeight / 2), (float)calcWidth, (float)calcHeight, _imgs[heartKey]);
            
                }
            }
                                
        }

        private void CreateNewHeart(ICollection<int> newIDs, int newID)
        {
            var curKey = -1;
            foreach (var heartKey in _imgs.Keys.Where(heartKey => !newIDs.Contains(heartKey)))
            {
                curKey = heartKey;
                break;
            }
            if(curKey>= 0)
            {
                _imgs[newID] = _imgs[curKey];
                _imgs.Remove(curKey);
            }else
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
            image.UriSource = new Uri(@"pack://application:,,,/Resources/heart.gif", UriKind.RelativeOrAbsolute);
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
            return joint.TrackingState == JointTrackingState.Tracked || joint.TrackingState == JointTrackingState.Inferred;
        }

        private bool SkeletTracked(Skeleton skelet)
        {
            var tracked = skelet.TrackingState == SkeletonTrackingState.Tracked;
            return tracked;
        }


    }
}
