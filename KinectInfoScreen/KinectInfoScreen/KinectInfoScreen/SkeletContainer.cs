using System.Collections.Generic;
using System.Threading;
using FarseerPhysics.Dynamics;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace KinectInfoScreen
{
    internal class SkeletContainer
    {
        public World World { get; set; }

        public SkeletContainer(World world)
        {
            World = world;
            Joints = new Dictionary<JointType, SkeletonPoint>();
            GroundLine = 0;
            DeleteTimer = new Timer(DeleteSkelets, null, 1000, Timeout.Infinite);
        }

        public void CreateRagdoll(PhysicsGameScreen screen)
        {
            if (Ragdoll != null) return;
            Ragdoll = new Ragdoll(World, screen, GetRagdolPos());
        }

        private Vector2 GetRagdolPos()
        {
            return new Vector2(0, 0);
        }

        private void DeleteSkelets(object state)
        {
            var doll = Ragdoll;
            Ragdoll = null;
            doll.Remove(World);
            //TODO event damit aus List Raus
        }

        public Dictionary<JointType, SkeletonPoint> Joints { get; set; }
        public float GroundLine { get; set; }
        public Ragdoll Ragdoll { get; set; }
        private Timer DeleteTimer { get; set; }

        public void UpdateDeleteTimer()
        {
            var timer = DeleteTimer;
            timer.Change(1000, Timeout.Infinite);
        }
    }
}