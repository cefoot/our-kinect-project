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

        void InitMenuItems()
        {
            items.Add(new KinectMenuItem { LabelText = "\r\nCorBene\r\n" });
            items.Add(new KinectMenuItem { LabelText = "\r\nexpert Connect\r\n" });
            items.Add(new KinectMenuItem { LabelText = "\r\nItem1\r\n" });
            items.Add(new KinectMenuItem { LabelText = "\r\nItem2\r\n" });
            items.ToList().ForEach(itm =>
            {
                itm.Visibility = System.Windows.Visibility.Hidden;
                itm.Background = new SolidColorBrush(Colors.Green) { Opacity = .25d };
                gridContainer.Children.Add(itm);
                Grid.SetRowSpan(itm, 2);
                Grid.SetColumnSpan(itm, 3);
                itm.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                itm.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                itm.MouseDoubleClick += itm_MouseDoubleClick;
            });
        }

        void itm_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var itm = sender as KinectMenuItem;
            if (itm.LabelText.Contains("Item1"))
            {
                var element = new MediaElement { Source = new Uri(@"p:\telematik\ExpertConnect_v2.mp4") };
                element.MediaEnded += (send, args) =>
                {
                    gridContainer.Children.Remove(element);
                };
                Grid.SetColumnSpan(element, 3);
                Grid.SetRowSpan(element, 2);
                element.Width = gridContainer.ActualWidth;
                element.Height = gridContainer.ActualHeight;
                gridContainer.Children.Add(element);
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

    }
}
