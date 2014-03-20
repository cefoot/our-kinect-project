using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitKinectSensor();
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
                image1.Source = new DrawingImage(myDrawingGroup);
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

        private Image CreateHeart()
        {
            var heartImg = new Image();
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(@"pack://application:,,,/Resources/giphy.gif", UriKind.RelativeOrAbsolute);
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
