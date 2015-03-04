using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.Toolkit.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace De.DataExperts.conhITApp
{
    public partial class MainWindow : IKinectEngagementManager
    {
        public event EventHandler Engaged;
        public event EventHandler Disengaged;

        public bool EngagedBodyHandPairsChanged()
        {
            return (this.changed);
        }
        public IReadOnlyList<BodyHandPair> KinectManualEngagedHands
        {
            get
            {
                return (KinectCoreWindow.KinectManualEngagedHands);
            }
        }
        public void StartManaging()
        {
            this.bodies = new Body[this.kinect.BodyFrameSource.BodyCount];
            this.reader = this.kinect.BodyFrameSource.OpenReader();
            this.reader.FrameArrived += OnFrameArrived;
        }
        public void StopManaging()
        {
            this.reader.FrameArrived -= OnFrameArrived;
            this.reader.Dispose();
            this.bodies = null;
        }
        static bool AreTracked(Body body, params JointType[] joints)
        {
            return (joints.All(j => body.Joints[j].TrackingState == TrackingState.Tracked));
        }
        static double VerticalDistance(Body body, JointType jointOne, JointType jointTwo)
        {
            return (body.Joints[jointOne].Position.Y - body.Joints[jointTwo].Position.Y);
        }
        void OnFrameArrived(Object sender, BodyFrameArrivedEventArgs args)
        {
            if (args.FrameReference != null)
            {
                using (var frame = args.FrameReference.AcquireFrame())
                {
                    if (frame == null) return;
                    frame.GetAndRefreshBodyData(this.bodies);

                    // we want the first body that is tracked and where the 2 joints I'm interested  
                    // in are tracked and where the hand is above the shoulder.  
                    var engaging = false;
                    foreach (var bdy in from curBdy in bodies where curBdy.IsTracked select curBdy)
                    {
                        if (AreTracked(bdy, JointType.HandRight, JointType.ShoulderRight)
                                && VerticalDistance(bdy, JointType.HandRight, JointType.ShoulderRight) >= 0.0d)
                        {
                            EnsureEngaged(bdy.TrackingId, HandType.RIGHT);
                            engaging = true;
                            break;
                        }
                        else if (AreTracked(bdy, JointType.HandLeft, JointType.ShoulderLeft)
                                && VerticalDistance(bdy, JointType.HandLeft, JointType.ShoulderLeft) >= 0.0d)
                        {
                            EnsureEngaged(bdy.TrackingId, HandType.LEFT);
                            engaging = true;
                        }
                        if (engaging)
                        {
                            break;
                        }
                    }
                    // got one? check to see if we need to trigger engagement.  
                    if (!engaging)
                    {
                        // not got one? check to see if we need to clear engagement.  
                        this.EnsureNotEngaged();
                    }
                }
            }
        }
        void EnsureEngaged(ulong trackingId, HandType hand)
        {
            if (this.trackingId != trackingId)
            {
                this.trackingId = trackingId;

                this.changed = true;

                KinectCoreWindow.SetKinectOnePersonManualEngagement(
                  new BodyHandPair(trackingId, hand));

                if (this.Engaged != null)
                {
                    this.Engaged(this, EventArgs.Empty);
                }
            }
        }
        void EnsureNotEngaged()
        {
            if (this.trackingId != null)
            {
                this.changed = true;

                this.trackingId = null;

                KinectCoreWindow.SetKinectOnePersonManualEngagement(null);

                if (this.Disengaged != null)
                {
                    this.Disengaged(this, EventArgs.Empty);
                }
            }
        }

        bool changed = false;
        ulong? trackingId;
        
        BodyFrameReader reader;
        Body[] bodies;
    }
}
