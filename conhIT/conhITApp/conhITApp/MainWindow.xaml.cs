using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        private bool _debugging = false;

        private ThinkBubble bubble;

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.H:
                    MessageBox.Show("Folgende Hotkeys sind hinterlegt:\n\tF\tVollbild\n\tH\tHilfe\n\tESC\tSchließen", "Hilfe");
                    break;
                case Key.X:
                    _debugging = !_debugging;
                    MessageBox.Show(String.Format("Debug-Schalter steht jetzt auf: '{0}'", _debugging));
                    break;
                case Key.V:
                    if (!_debugging) break;
                    //MoveHeart(450f, 50f, 50f, 50f, CreateHeart());
                    StartVideo(new Uri(@"p:\Messen\conhIT\2015\expertconnect_professor.mp4"));
                    break;
                case Key.Y:
                    if (!_debugging) break;
                    //MoveHeart(450f, 50f, 50f, 50f, CreateHeart());
                    ShowImage(new BitmapImage(new Uri(@"p:\Temp\cpapenfuss\conHIT\xout.GIF")));
                    break;
                case Key.F:
                    WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
                    break;
                case Key.T:
                    if (!_debugging) break;
                    var pos = Mouse.GetPosition(gridContainer);
                    bubble = ShowAndCreateThinkbubble(pos, bubble);
                    break;
                case Key.M:
                    if (!_debugging) break;
                    ShowUserMenu(new System.Windows.Point(this.ActualWidth / 2d, this.ActualHeight / 2d), 200d);
                    break;
                case Key.Escape:
                    Application.Current.Shutdown();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(kinectRegion.CursorSpriteSheetDefinition.ImageUri);
            kinectRegion.CursorSpriteSheetDefinition = new CursorSpriteSheetDefinition(
                new Uri(@"pack://application:,,,/Resources/cursorspritesheet.png", UriKind.RelativeOrAbsolute),
                kinectRegion.CursorSpriteSheetDefinition.Columns,
                kinectRegion.CursorSpriteSheetDefinition.Rows,
                kinectRegion.CursorSpriteSheetDefinition.SpriteWidth,
                kinectRegion.CursorSpriteSheetDefinition.SpriteHeight);
            var bitmap = Properties.Resources.face;
            bitmap.MakeTransparent(System.Drawing.Color.Blue);
            _face = CreateBitmapSourceFromBitmap(bitmap);
            try
            {
                InitMenuItems();
            }
            catch (Exception)
            {
                MessageBox.Show(String.Format("Kann die Datei '{0}' kan nicht geladen werden.", new FileInfo("MenuItems.txt").FullName));
                throw;
            }
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
