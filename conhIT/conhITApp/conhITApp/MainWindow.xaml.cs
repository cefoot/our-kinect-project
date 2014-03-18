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
using Microsoft.Kinect;

namespace conhITApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitKinectSensor();
        }

        private void InitKinectSensor()
        {
            this.kinect = KinectSensor.KinectSensors.Where(x => x.Status == KinectStatus.Connected).FirstOrDefault();
            this.kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(Sensor_SkeletonFrameReady);
            this.kinect.SkeletonStream.Enable();
            this.kinect.Start();
        }

        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {

                    Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];

                    frame.CopySkeletonDataTo(skeletons);

                    foreach (Skeleton skeleton in skeletons.Where(SkeletTracked))
                    {
                        if (skeleton != null)
                        {

                            if (IsTrackedOrInferred(skeleton.Joints[JointType.Spine]) && IsTrackedOrInferred(skeleton.Joints[JointType.ShoulderLeft]))
                            {
                                
                            }
                        }
                    }
                }

            }

        }

        private bool IsTrackedOrInferred(Joint joint)
        {
            return joint.TrackingState == JointTrackingState.Tracked || joint.TrackingState == JointTrackingState.Inferred;
        }

        

        private bool SkeletTracked(Skeleton skelet)
        {
            var tracked = skelet.TrackingState == SkeletonTrackingState.Tracked;
            return tracked;
        }

        
    }
}
