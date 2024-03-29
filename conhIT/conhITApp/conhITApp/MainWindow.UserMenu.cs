﻿using De.DataExperts.conhITApp;
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
using System.Threading;
using System.Windows.Threading;

namespace De.DataExperts.conhITApp
{
    public partial class MainWindow
    {
        List<KinectMenuItem> menuCtrls = new List<KinectMenuItem>();

        private List<FrameworkElement> imgCtrls = new List<FrameworkElement>();

        private List<FrameworkElement> vidCtrls = new List<FrameworkElement>();

        const string CSV_HEADER_LINE = "MENU_NAME;MENU_ACTION";

        void InitMenuItems()
        {
            using (var fReader = (File.Exists("MenuItems.txt") ? File.OpenRead("MenuItems.txt") : File.OpenRead("MenuItems.txt.sample")))
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
                    try
                    {
                        menuCtrls.Add(LoadMenuItem(data));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(String.Format("Error Loading MenuItems line:{0}", line), e);
                    }
                }
            }
            menuCtrls.ToList().ForEach(itm =>
            {
                itm.Visibility = System.Windows.Visibility.Hidden;
                gridContainer.Children.Add(itm);
                Grid.SetRowSpan(itm, 2);
                Grid.SetColumnSpan(itm, 3);
                itm.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                itm.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                itm.MouseDoubleClick += MenuItm_MouseDoubleClick;
            });
        }

        private Image CreateGesture()
        {
            var gestureImg = new Image();
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(@"pack://application:,,,/Resources/Grap_Gesture.gif", UriKind.RelativeOrAbsolute);
            image.EndInit();
            ImageBehavior.SetAnimatedSource(gestureImg, image);
            Grid.SetColumnSpan(gestureImg, 3);
            Grid.SetRowSpan(gestureImg, 2);
            gestureImg.VerticalAlignment = VerticalAlignment.Top;
            gestureImg.HorizontalAlignment = HorizontalAlignment.Left;
            gridContainer.Children.Add(gestureImg);
            return gestureImg;
        }

        private void MoveGesture(float x, float y, float height, float width, Image gestureImg)
        {
            gestureImg.Width = width;
            gestureImg.Height = height;
            gestureImg.Margin = new Thickness(x, y, 0, 0);
        }

        private static KinectMenuItem LoadMenuItem(string[] data)
        {
            var newItm = new KinectMenuItem();// { LabelText = data[0].Replace("\\n", Environment.NewLine) };
            Panel.SetZIndex(newItm, 1);//über dem Rest
            var lblCont = data[0].Replace("\\n", Environment.NewLine);
            FileInfo file;

            try
            {
                if ((file = new FileInfo(lblCont)).Exists)
                {
                    newItm.LabelImage = new BitmapImage(new Uri(file.FullName));
                }
                else
                {
                    newItm.LabelText = lblCont;
                }
            }
            catch
            {
                newItm.LabelText = lblCont;
            }

            if ((file = new FileInfo(data[1])).Exists)
            {
                try
                {
                    var img = new BitmapImage(new Uri(file.FullName));
                    newItm.Tag = img;
                }
                catch (Exception)
                {
                    //so it's a video file
                    newItm.Tag = new Uri(file.FullName);
                }
            }
            return newItm;
        }

        void MenuItm_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var itm = sender as KinectMenuItem;
            userExperienced = true;//er hat ein Menupunkt geklickt
            menuCtrls.ToList().ForEach(cItm =>
            {
                cItm.IsForceHide = true;
                cItm.Visibility = System.Windows.Visibility.Hidden;
            });
            if (itm.Tag is Uri)
            {
                StartVideo(itm.Tag as Uri);
            }
            else
            {
                ShowImage(itm.Tag as BitmapImage);
            }
        }

        private void ShowImage(BitmapImage bitmapImage)
        {
            if (imgCtrls.Count == 0)
            {
                var element = new Image();
                element.Stretch = Stretch.Uniform;
                KinectMenuItem newItm = new KinectMenuItem { LabelText = "Close" };
                newItm.MouseDoubleClick += (send, args) => HideImage();
                newItm.MouseDoubleClick += (send, args) => menuCtrls.ToList().ForEach(cItm =>
                {
                    cItm.IsForceHide = false;
                    cItm.Visibility = System.Windows.Visibility.Visible;
                });
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

        DispatcherTimer _progressTimer = new DispatcherTimer();

        private void StartVideo(Uri videoSrc)
        {
            if (vidCtrls.Count == 0)
            {
                //Video
                var element = new MediaElement();
                element.LoadedBehavior = MediaState.Manual;
                element.Loaded += (send, args) =>
                {
                    element.Play();
                    _progressTimer.Start();
                };
                element.MediaEnded += (send, args) =>
                {
                    menuCtrls.ToList().ForEach(cItm =>
                    {
                        cItm.IsForceHide = false;
                        cItm.Visibility = System.Windows.Visibility.Visible;
                    });
                    StopVideo();
                };
                Grid.SetColumnSpan(element, 3);
                Grid.SetRowSpan(element, 2);
                element.Margin = new Thickness(0);
                vidCtrls.Add(element);
                //Close Btn
                KinectMenuItem newItm = new KinectMenuItem { LabelText = "Close" };
                newItm.MouseDoubleClick += (send, args) => menuCtrls.ToList().ForEach(cItm =>
                {
                    cItm.IsForceHide = false;
                    cItm.Visibility = System.Windows.Visibility.Visible;
                });
                newItm.MouseDoubleClick += (send, args) => StopVideo();
                Grid.SetColumnSpan(newItm, 3);
                Grid.SetRowSpan(newItm, 2);
                newItm.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                newItm.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newItm.Margin = new Thickness(10, 10, 0, 0);
                vidCtrls.Add(newItm);
                //ProgressBar
                var progress = new ProgressBar();
                Grid.SetColumnSpan(progress, 3);
                Grid.SetRow(progress, 1);
                progress.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                progress.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                progress.Height = 20d;
                progress.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x19, 0x5D, 0x78));
                progress.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
                progress.BorderThickness = new Thickness(0d);
                vidCtrls.Add(progress);
                //timer
                _progressTimer.Interval = TimeSpan.FromMilliseconds(100);
                _progressTimer.Tick += UpdateVidProgressBar;
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
            var curVid = (from ctrls in vidCtrls where ctrls is MediaElement select ctrls as MediaElement).FirstOrDefault();
            curVid.Source = videoSrc;
        }

        private void UpdateVidProgressBar(object sender, EventArgs e)
        {
            var vid = (from ctrls in vidCtrls where ctrls is MediaElement select ctrls as MediaElement).FirstOrDefault();
            var progressBar = (from ctrls in vidCtrls where ctrls is ProgressBar select ctrls as ProgressBar).FirstOrDefault();
            var start = DateTime.Now;
            if (!vid.NaturalDuration.HasTimeSpan)
            {//läuft das Video Schon?
                return;
            }
            var runTime = vid.NaturalDuration.TimeSpan.TotalSeconds;
            if (runTime != progressBar.Maximum)
            {
                progressBar.Maximum = runTime;
            }
            progressBar.Value = vid.Position.TotalSeconds;
        }

        private void StopVideo()
        {
            _progressTimer.Stop();
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
                                where sk.TrackingId == this.curTrackedID
                                where sk.IsTracked
                                select sk).FirstOrDefault();
                    if (user == null)
                    {
                        menuCtrls.ToList().ForEach(itm => itm.Visibility = System.Windows.Visibility.Hidden);
                        ShowThinkbubble(skeletons, frame.RelativeTime);
                        return;
                    }
                    else
                    {
                        HideThinkbubble(skeletons);
                    }

                    if (!userExperienced && (DateTime.Now - userFirstEngaged).TotalSeconds > Properties.Settings.Default.Seconds2Wait_HelpGrab)
                    {
                        ShowHelpBubble(user);
                    }
                    else
                    {
                        HideHelpBubble();
                    }

                    if (curUserInactive)
                    {
                        menuCtrls.ToList().ForEach(itm => itm.Visibility = System.Windows.Visibility.Hidden);
                        return;
                    }

                    var pos = user.Joints[JointType.SpineShoulder].Position.GetControlPoint(kinect, new Size(gridContainer.ActualWidth, gridContainer.ActualHeight));
                    var dist = (user.Joints[JointType.ShoulderLeft].Position.GetControlPoint(kinect, new Size(gridContainer.ActualWidth, gridContainer.ActualHeight)) - user.Joints[JointType.ShoulderRight].Position.GetControlPoint(kinect, new Size(gridContainer.ActualWidth, gridContainer.ActualHeight))).Length;
                    if (dist > 500d || dist < 50d)
                    {
                        return;
                    }
                    ShowUserMenu(pos, dist);

                }

            }

        }

        private void ShowUserMenu(Point pos, double dist)
        {
            var singleAngle = 360d / menuCtrls.Count;
            var curAngle = 30d;
            menuCtrls.ToList().ForEach(itm =>
            {
                itm.Visibility = System.Windows.Visibility.Visible;
                var pnt = curAngle.ComputeCartesianCoordinate(dist);
                curAngle += singleAngle;
                pnt.Offset(pos.X, pos.Y);
                pnt.Offset(itm.ActualWidth / -2d, itm.ActualHeight / -2d);

                if (!double.IsNaN(pnt.X) && !double.IsNaN(pnt.Y) && !double.IsInfinity(pnt.X) && !double.IsInfinity(pnt.Y))
                {
                    itm.Margin = new Thickness(pnt.X, pnt.Y, 0d, 0d);
                }
            });
        }

        private class VisibleBody
        {
            public TimeSpan StartTime { get; set; }
            public ThinkBubble Bubble { get; set; }
        }

        Dictionary<ulong, VisibleBody> visileBdys = new Dictionary<ulong, VisibleBody>();

        private void HideThinkbubble(Body[] skeletons)
        {
            visileBdys.Values.ToList().ForEach(bdy => gridContainer.Children.Remove(bdy.Bubble));
            visileBdys.Clear();
        }

        private void ShowThinkbubble(Body[] skeletons, TimeSpan time)
        {
            var user = (from sk in skeletons
                        where sk.IsTracked
                        select sk).ToList();
            foreach (var skelet in user)
            {
                if (visileBdys.ContainsKey(skelet.TrackingId))
                {
                    if ((time - visileBdys[skelet.TrackingId].StartTime).TotalSeconds > Properties.Settings.Default.Seconds2Wait_HelpActivate)
                    {
                        var pos = skelet.Joints[JointType.Head].Position.GetControlPoint(kinect, new Size(image1.ActualWidth, image1.ActualHeight));
                        pos = image1.PointToScreen(pos);
                        pos = gridContainer.PointFromScreen(pos);
                        visileBdys[skelet.TrackingId].Bubble = ShowAndCreateThinkbubble(pos, visileBdys[skelet.TrackingId].Bubble);
                    }
                }
                else
                {
                    var bdy = new VisibleBody();
                    bdy.StartTime = time;
                    visileBdys[skelet.TrackingId] = bdy;
                }
            }
            var goneUsers = (from vis in visileBdys
                             where !user.Exists(usr => usr.TrackingId == vis.Key)
                             select vis).ToList();
            foreach (var goneUser in goneUsers)
            {
                visileBdys.Remove(goneUser.Key);
                gridContainer.Children.Remove(goneUser.Value.Bubble);
            }

        }

        private HelpThinkBubble helpBubble;

        private void HideHelpBubble()
        {
            if (helpBubble == null) return;
            helpBubble.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ShowHelpBubble(Body user)
        {
            var pos = user.Joints[JointType.Head].Position.GetControlPoint(kinect, new Size(image1.ActualWidth, image1.ActualHeight));
            pos = image1.PointToScreen(pos);
            pos = gridContainer.PointFromScreen(pos);
            helpBubble = ShowAndCreateThinkbubble(pos, helpBubble);
            helpBubble.Visibility = System.Windows.Visibility.Visible;
        }

        private T ShowAndCreateThinkbubble<T>(Point pos, T thinkBubble)
           where T : FrameworkElement
        {
            if (thinkBubble == null)
            {
                thinkBubble = Activator.CreateInstance<T>();
                gridContainer.Children.Add(thinkBubble);
                Grid.SetColumnSpan(thinkBubble, 3);
                Grid.SetRowSpan(thinkBubble, 2);
                thinkBubble.VerticalAlignment = VerticalAlignment.Top;
                thinkBubble.HorizontalAlignment = HorizontalAlignment.Left;
            }

            thinkBubble.Margin = new Thickness(pos.X, pos.Y, 0, 0);
            return thinkBubble;
        }

    }
}
