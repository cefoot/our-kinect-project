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
        private List<SkeletData> _skeletData = new List<SkeletData>();

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
                    Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];

                    frame.CopySkeletonDataTo(skeletons);
                    this._skeletData = new List<SkeletData>();
                    foreach (Skeleton skeleton in skeletons.Where(SkeletTracked))
                    {
                        var skeletDataObject = new SkeletData();
                        if (skeleton != null)
                        {
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
                                //Debug.WriteLine(skeletDataObject.heartWidth);
                                var calcWidth = container.ActualWidth * (skeletDataObject.heartWidth / kinect.ColorStream.FrameWidth);
                                calcWidth = Math.Max(calcWidth, 34);
                                var calcHeight = calcWidth;//bild ist genauso hoch wie breit
                                //Debug.WriteLine(calcHeight);
                                CreateHeart((float)(container.ActualWidth * skeletDataObject.HeartPosition.X / kinect.ColorStream.FrameWidth - calcWidth / 2), (float)(container.ActualHeight * skeletDataObject.HeartPosition.Y / kinect.ColorStream.FrameHeight - calcHeight / 2), (float)calcWidth, (float)calcHeight);
                                _skeletData.Add(skeletDataObject);

                            }
                        }
                    }
                }

            }

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
        Image heartImg;
        private void CreateHeart(float x, float y, float height, float width)
        {
            if (heartImg == null)
            {
                heartImg = new Image();
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(@"pack://application:,,,/Resources/giphy.gif", UriKind.RelativeOrAbsolute);
                image.EndInit();
                ImageBehavior.SetAnimatedSource(heartImg, image);
                heartImg.VerticalAlignment = VerticalAlignment.Top;
                heartImg.HorizontalAlignment = HorizontalAlignment.Left;
                container.Children.Add(heartImg);
            }
            heartImg.Width = width;
            heartImg.Height = height;
            heartImg.Margin = new Thickness(x, y, 0, 0);
        }


    }
}
