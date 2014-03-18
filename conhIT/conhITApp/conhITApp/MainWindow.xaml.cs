using System;
using System.Collections.Generic;
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
            this.kinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
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
                        SkeletData skeletDataObject = new SkeletData();
                        if (skeleton != null)
                        {
                            var spine = skeleton.Joints[JointType.Spine];
                            var shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
                            if (IsTrackedOrInferred(spine) && IsTrackedOrInferred(shoulderLeft))
                            {
                                //create point in between
                                var heartPosition = middlePoint(spine.Position, shoulderLeft.Position);
                                skeletDataObject.HeartPosition = this.kinect.CoordinateMapper.MapSkeletonPointToColorPoint(heartPosition, ColorImageFormat.RgbResolution640x480Fps30);
                                skeletDataObject.HeartDistance = heartPosition.Z;
                                CreateHeart(heartPosition.X,heartPosition.Y,50,50);
                                this._skeletData.Add(skeletDataObject);

                            }
                        }
                    }
                }

            }

        }

        private SkeletonPoint middlePoint(SkeletonPoint a, SkeletonPoint b)
        {
            SkeletonPoint result=new SkeletonPoint();
            result.X = a.X + 0.5f * (b.X - a.X);
            result.Y = a.Y + 0.5f * (b.Y - a.Y);
            result.Z = a.Z + 0.5f * (b.Z - a.Z);
            return result;
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
                heartImg.Width = width;
                heartImg.Height = height;
                container.Children.Add(heartImg);
            }
            heartImg.Margin = new Thickness(x, y, 0, 0);
        }


    }
}
