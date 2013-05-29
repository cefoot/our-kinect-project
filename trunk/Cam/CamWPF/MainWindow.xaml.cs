using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using Color = System.Drawing.Color;
using Size = System.Windows.Size;

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

        private BitmapSource _face;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
            nui.Start();
            var bitmap = Properties.Resources.face;
            bitmap.MakeTransparent(Color.Blue);
            _face = CreateBitmapSourceFromBitmap(bitmap);
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
        private void SetupKinect()
        {
            if (KinectSensor.KinectSensors.Count == 0)
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
                _heads = skelets.Select(skeleton => skeleton.Joints[JointType.Head]).Where(head => head.TrackingState == JointTrackingState.Tracked).ToList();
            }
        }

        void NuiColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var image = e.OpenColorImageFrame())
            {
                if (image == null) return;
                var data = new byte[image.PixelDataLength];
                image.CopyPixelDataTo(data);
                var myDrawingGroup = new DrawingGroup();
                myDrawingGroup.Children.Add(new ImageDrawing(data.ToBitmapSource(image.Width, image.Height), new Rect(new Size(image.Width, image.Height))));
                var joints = _heads.ToArray();
                foreach (var headPos in joints.Select(head => nui.CoordinateMapper.MapSkeletonPointToColorPoint(head.Position, ColorImageFormat.RgbResolution640x480Fps30)))
                {
                    myDrawingGroup.Children.Add(new ImageDrawing(_face, new Rect(headPos.X-40, headPos.Y-42, 80, 84)));
                }
                image1.Source = new DrawingImage(myDrawingGroup);
            }
        }

    }
}
