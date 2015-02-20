using De.DataExperts.conhITApp;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace De.DataExperts.conhITApp
{
    public partial class MainWindow
    {
        HashSet<KinectMenuItem> items = new HashSet<KinectMenuItem>();

        const string CSV_HEADER_LINE = "MENU_NAME;MENU_ACTION";

        void InitMenuItems()
        {
            using (var fReader = File.OpenRead("MenuItems.txt"))
            using (var reader = new StreamReader(fReader))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(CSV_HEADER_LINE))
                    {
                        continue;
                    }
                    var data = line.Split(";".ToCharArray(), 2);
                    items.Add(LoadMenuItem(data));
                }
            }
            items.ToList().ForEach(itm =>
            {
                itm.Visibility = System.Windows.Visibility.Hidden;
                //itm.Background = new SolidColorBrush(Colors.Green) { Opacity = .25d };
                gridContainer.Children.Add(itm);
                Grid.SetRowSpan(itm, 2);
                Grid.SetColumnSpan(itm, 3);
                itm.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                itm.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                itm.MouseDoubleClick += MenuItm_MouseDoubleClick;
            });
        }

        private static KinectMenuItem LoadMenuItem(string[] data)
        {
            var newItm = new KinectMenuItem { LabelText = data[0] };
            try
            {
                var img = new BitmapImage(new Uri(data[1]));
                newItm.Tag = img; 
            }
            catch (Exception)
            {
                //so it's a video file
                newItm.Tag = new Uri(data[1]);
            }
            return newItm;
        }

        void MenuItm_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var itm = sender as KinectMenuItem;
            if (itm.Tag is Uri)
            {
                StartVideo(itm.Tag as Uri);
            }
            else
            {
                ShowImage(itm.Tag as BitmapImage);
            }
        }

        List<FrameworkElement> imgCtrls = new List<FrameworkElement>();

        private void ShowImage(BitmapImage bitmapImage)
        {
            if (imgCtrls.Count == 0)
            {
                var element = new Image();
                element.Stretch = Stretch.Uniform;
                KinectMenuItem newItm = new KinectMenuItem { LabelText = "Close" };
                newItm.MouseDoubleClick += (send, args) => HideImage();
                Grid.SetColumnSpan(element, 3);
                Grid.SetRowSpan(element, 2);
                element.Margin = new Thickness(0);
                imgCtrls.Add(element);
                Grid.SetColumnSpan(newItm, 3);
                Grid.SetRowSpan(newItm, 2);
                newItm.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                newItm.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newItm.Margin = new Thickness(10, 10, 0, 0);
                imgCtrls.Add(newItm);
            }
            if (gridContainer.Children.Contains(imgCtrls[0]))
            {
                //image is showing
                return;
            }
            foreach (var ctrl in imgCtrls)
            {
                gridContainer.Children.Add(ctrl);
            }
            (from ctrls in imgCtrls where ctrls is Image select ctrls as Image).FirstOrDefault().Source = bitmapImage;
        }

        private void HideImage()
        {
            foreach (var ctrl in imgCtrls)
            {
                if (ctrl is Image)
                {
                    (ctrl as Image).Source = null;
                }
                gridContainer.Children.Remove(ctrl);
            }
        }

        List<FrameworkElement> vidCtrls = new List<FrameworkElement>();

        private void StartVideo(Uri videoSrc)
        {
            if (vidCtrls.Count == 0)
            {
                var element = new MediaElement();// { Source = new Uri(@"p:\telematik\ExpertConnect_v2.mp4") };
                element.LoadedBehavior = MediaState.Manual;
                element.Loaded += (send, args) => element.Play();
                KinectMenuItem newItm = new KinectMenuItem { LabelText = "Close" };
                newItm.MouseDoubleClick += (send, args) => StopVideo();
                element.MediaEnded += (send, args) => StopVideo();
                Grid.SetColumnSpan(element, 3);
                Grid.SetRowSpan(element, 2);
                element.Margin = new Thickness(0);
                vidCtrls.Add(element);
                Grid.SetColumnSpan(newItm, 3);
                Grid.SetRowSpan(newItm, 2);
                newItm.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                newItm.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newItm.Margin = new Thickness(10, 10, 0, 0);
                vidCtrls.Add(newItm);
            }
            if (gridContainer.Children.Contains(vidCtrls[0]))
            {
                //video is running
                return;
            }
            foreach (var ctrl in vidCtrls)
            {
                gridContainer.Children.Add(ctrl);
            }
            (from ctrls in vidCtrls where ctrls is MediaElement select ctrls as MediaElement).FirstOrDefault().Source = videoSrc;
        }

        private void StopVideo()
        {
            foreach (var ctrl in vidCtrls)
            {
                if (ctrl is MediaElement)
                {
                    (ctrl as MediaElement).Close();
                }
                gridContainer.Children.Remove(ctrl);
            }
        }


        void Sensor_SkeletonFrameReady_UserMenu(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var skeletons = new Body[frame.BodyCount];

                    frame.GetAndRefreshBodyData(skeletons);
                    var user = (from sk in skeletons
                                where sk.TrackingId == this.trackingId
                                where sk.IsTracked
                                select sk).FirstOrDefault();
                    if (user == null)
                    {
                        items.ToList().ForEach(itm => itm.Visibility = System.Windows.Visibility.Hidden);
                        ShowThinkbubble(skeletons, frame.RelativeTime);
                        return;
                    }

                    var pos = user.Joints[JointType.SpineShoulder].Position.GetControlPoint(kinect, new Size(gridContainer.ActualWidth, gridContainer.ActualHeight));
                    var dist = (user.Joints[JointType.ShoulderLeft].Position.GetControlPoint(kinect, new Size(gridContainer.ActualWidth, gridContainer.ActualHeight)) - user.Joints[JointType.ShoulderRight].Position.GetControlPoint(kinect, new Size(gridContainer.ActualWidth, gridContainer.ActualHeight))).Length;
                    if (dist > 500d || dist < 50d)
                    {
                        return;
                    }
                    var singleAngle = 360d / items.Count;
                    var curAngle = 30d;
                    items.ToList().ForEach(itm =>
                    {
                        itm.Visibility = System.Windows.Visibility.Visible;
                        var pnt = curAngle.ComputeCartesianCoordinate(dist);
                        curAngle += singleAngle;
                        pnt.Offset(pos.X, pos.Y);
                        pnt.Offset(itm.ActualWidth / -2d, itm.ActualHeight / -2d);
                        itm.Margin = new Thickness(pnt.X, pnt.Y, 0d, 0d);
                    });

                }

            }

        }

        Dictionary<ulong, TimeSpan> visileBdys = new Dictionary<ulong, TimeSpan>();

        private void ShowThinkbubble(Body[] skeletons, TimeSpan time)
        {
            var user = from sk in skeletons
                                where sk.IsTracked
                                select sk;
            foreach (var skelet in user)
            {
                if (visileBdys.ContainsKey(skelet.TrackingId))
                {
                    if ((time - visileBdys[skelet.TrackingId]).TotalMilliseconds > 1000)
                    {
                        Console.WriteLine("Lange Da");
                    }
                }
                else
                {
                    visileBdys[skelet.TrackingId] = time;
                }
            }

        }

    }
}
