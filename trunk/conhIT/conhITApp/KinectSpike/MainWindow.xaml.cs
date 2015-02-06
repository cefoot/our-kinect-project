using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace De.DataExperts.conhITApp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor kinect;

        public MainWindow()
        {
            InitializeComponent();
            kinect = KinectSensor.GetDefault();
            kinectRegion.KinectSensor = kinect;
            imageFormat = this.kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.colorBitmap = new WriteableBitmap(imageFormat.Width, imageFormat.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            var bodyReader = kinect.BodyFrameSource.OpenReader();
            bodyReader.FrameArrived += Sensor_SkeletonFrameReady;
            bodyReader.FrameArrived += Sensor_SkeletonFrameReady_UserMenu;
            kinect.ColorFrameSource.OpenReader().FrameArrived += Sensor_ColorFrameReady;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var bitmap = Properties.Resources.face;
            bitmap.MakeTransparent(System.Drawing.Color.Blue);
            _face = CreateBitmapSourceFromBitmap(bitmap);
            InitMenuItems();
        }

        private void kinectRegion_Loaded(object sender, RoutedEventArgs e)
        {
            kinectRegion.SetKinectOnePersonManualEngagement(this);
            
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinect.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked");//Funzt
        }

        private void btn1_MouseMove(object sender, MouseEventArgs e)
        {
            MessageBox.Show("mouse over");
        }

        private void btn1_MouseEnter(object sender, MouseEventArgs e)
        {
            MessageBox.Show("btn1_MouseEnter");
        }

        private void btn1_MouseLeave(object sender, MouseEventArgs e)
        {
            MessageBox.Show("btn1_MouseLeave");
        }


    }
}
