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
            }
        }

        void NuiColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var image = e.OpenColorImageFrame())
            {
                if(image == null) return;
                var data = new byte[image.PixelDataLength];
                image.CopyPixelDataTo(data);
                image1.Source = data.ToBitmapSource(image.Width, image.Height);
            }
        }
    }
}
