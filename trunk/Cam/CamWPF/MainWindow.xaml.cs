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
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;

namespace CamWPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private KinectSensor nui;

        private List<Joint> _heads = new List<Joint>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
            nui.Start();
        }
        private void SetupKinect()
        {
            if (KinectSensor.KinectSensors.Count== 0)
            {
                this.Title = "No Kinect connected";
            }
            else
            {
                //use first Kinect
                nui = KinectSensor.KinectSensors[0];

                //Initialize to return both Color & Depth images
                nui.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);// .Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth));
                nui.ColorFrameReady += NuiColorFrameReady;
                nui.SkeletonStream.Enable();
                nui.SkeletonFrameReady += NuiSkeletonFrameReady;
            }
        }

        void NuiSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var skeletFrame = e.OpenSkeletonFrame())
            {
                var skelets = new Skeleton[skeletFrame.SkeletonArrayLength];
                skeletFrame.CopySkeletonDataTo(skelets);
                _heads = skelets.Select(skeleton => skeleton.Joints[JointType.Head]).ToList();
            }
        }

        ImageSourceConverter c = new ImageSourceConverter();

        void NuiColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var image = e.OpenColorImageFrame())
            {
                if(image == null) return;
                var data = new byte[image.PixelDataLength];
                image.CopyPixelDataTo(data);
                var myDrawingGroup = new DrawingGroup();
                myDrawingGroup.Children.Add(new ImageDrawing(data.ToBitmapSource(image.Width, image.Height), new Rect(new Size(image.Width, image.Height))));
                var joints = _heads.ToArray();
                foreach (var head in joints)
                {
                    var headPos = nui.CoordinateMapper.MapSkeletonPointToColorPoint(head.Position, ColorImageFormat.RgbResolution640x480Fps30);
                    myDrawingGroup.Children.Add(new ImageDrawing(c. Properties.Resources.face, new Rect(40, 0, 45, 130)));
                }
                image1.Source = new DrawingImage(myDrawingGroup);
            }
        }
    }
}
