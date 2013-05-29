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
using KinectUserHeight;
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

        private List<SkeletData> _skeletData = new List<SkeletData>();

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
                if (skeletFrame == null)
                {
                    return;
                }
                var skelets = new Skeleton[skeletFrame.SkeletonArrayLength];
                skeletFrame.CopySkeletonDataTo(skelets);
                var datas = new List<SkeletData>();
                foreach (var skeleton in skelets)
                {
                    if(skeleton.TrackingState != SkeletonTrackingState.Tracked) continue;
                    var data = new SkeletData();
                    var head = skeleton.Joints[JointType.Head];
                    data.HeadPosition = nui.CoordinateMapper.MapSkeletonPointToColorPoint(head.Position, ColorImageFormat.RgbResolution640x480Fps30);
                    data.HeadDistance = head.Position.Z;
                    data.Height = CalculateHeight(skeleton);
                    datas.Add(data);
                }
                _skeletData = datas;
            }
        }

        class SkeletData
        {
            public double Height { get; set; }
            public ColorImagePoint HeadPosition { get; set; }
            public float HeadDistance { get; set; }
        }

        private static double CalculateHeight(Skeleton skeleton)
        {
            if (skeleton != null)
            {
                // Calculate height.
                var height = Math.Round(skeleton.Height(), 2);
                return height;
            }
            return 0d;
        }

        const double defaultWidth = 80d;
        const double defaultHeight = 84d;

        void NuiColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var image = e.OpenColorImageFrame())
            {
                if (image == null) return;
                var data = new byte[image.PixelDataLength];
                image.CopyPixelDataTo(data);
                var myDrawingGroup = new DrawingGroup();
                myDrawingGroup.Children.Add(new ImageDrawing(data.ToBitmapSource(image.Width, image.Height), new Rect(new Size(image.Width, image.Height))));
                var skeletDatas = _skeletData.ToArray();
                foreach (var skeletData in skeletDatas)
                {
                    var calcWidth = 50d/(skeletData.HeadDistance/3d);
                    var calcHeight = 52.5d/(skeletData.HeadDistance/3d);

                    myDrawingGroup.Children.Add(new ImageDrawing(_face, new Rect(skeletData.HeadPosition.X - ((int) calcWidth/2), skeletData.HeadPosition.Y - ((int) calcHeight/2), calcWidth, calcHeight)));

                    tblHeight.Margin = new Thickness(skeletData.HeadPosition.X, skeletData.HeadPosition.Y, 0, 0);
                    tblHeight.Text = skeletData.Height.ToString();


                    //myDrawingGroup.Children.Add(new ImageDrawing(c. Properties.Resources.face, new Rect(40, 0, 45, 130)));
                }
                image1.Source = new DrawingImage(myDrawingGroup);
            }
        }

    }
}
