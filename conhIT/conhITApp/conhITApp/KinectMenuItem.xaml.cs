﻿using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace De.DataExperts.conhITApp
{
    /// <summary>
    /// Interaktionslogik für MenuItem.xaml
    /// </summary>
    public partial class KinectMenuItem : UserControl , IKinectControl
    {
        public KinectMenuItem()
        {
            InitializeComponent();
        }

        public IKinectController CreateController(Microsoft.Kinect.Toolkit.Input.IInputModel inputModel, KinectRegion kinectRegion)
        {
            //return new PressableController(this, (PressableModel)inputModel,kinectRegion);
            return new ManipulatableController(this, (ManipulatableModel)inputModel, kinectRegion);
        }

        public void Clicked()
        {
            RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Label.MouseDoubleClickEvent });
        }

        public bool IsForceHide { get; set; }

        public new Visibility Visibility
        {
            get
            {
                return base.Visibility;
            }
            set
            {
                if (value == System.Windows.Visibility.Hidden || !IsForceHide)
                {
                    base.Visibility = value;
                }
            }
        }

        public String LabelText
        {
            get
            {
                return txt.Content.ToString();
            }
            set
            {
                txt.Content = value;
                img.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public ImageSource LabelImage
        {
            get
            {
                return img.Source;
            }
            set
            {
                img.Source = value;
                var diameter = Math.Sqrt(img.Source.Width * img.Source.Width + img.Source.Height * img.Source.Height);
                var horizontal = (diameter - img.Source.Width) / 2d;
                var vertical = (diameter - img.Source.Height) / 2d;
                img.Margin = new Thickness(horizontal, vertical, horizontal, vertical);//make circle fit image size
                img.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public bool IsManipulatable
        {
            get { return true; }
        }

        public bool IsPressable
        {
            get { return false; }
        }

        private bool _ChangingSize { get; set; }

        private void Ctrl_SizeChanged(object sender, SizeChangedEventArgs e)
        {//controls should always be square
            if (_ChangingSize)
            {//guard
                _ChangingSize = false;
                return;
            }
            if (e.NewSize.IsEmpty)
            {
                return;
            }
            if (e.NewSize.Height >= e.NewSize.Width)
            {
                _ChangingSize = true;
                Width = e.NewSize.Height;
            }
            else if (e.NewSize.Width > e.NewSize.Height)
            {
                _ChangingSize = true;
                Height = e.NewSize.Width;
            }
        }
        private static Action EmptyDelegate = delegate() { };

    }
}
