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
            DeleteTimer = new Timer(DeleteSkelets, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void CreateRagdoll(PhysicsGameScreen screen)
        {
            if (Ragdoll != null) return;
            Ragdoll = new Ragdoll(World, screen, GetRagdolPos());
            Ragdoll.CollisionCategories = Category.Cat2;
            Ragdoll.CollidesWith = Ragdoll.CollisionCategories;
            UpdateDeleteTimer();
        }

        private Vector2 GetRagdolPos()
        {
            return new Vector2(0, 0);
        }

        private bool timerDisposed = false;

        private void DeleteSkelets(object state)
        {
            var timer = DeleteTimer;
            timerDisposed = true;
            timer.Dispose();
            var doll = Ragdoll;
            Ragdoll = null;
            DeleteTimer = null;
            doll.RemoveFrom(World);
            //TODO event damit aus List Raus
        }

        public Dictionary<JointType, SkeletonPoint> Joints { get; set; }
        public float GroundLine { get; set; }
        public Ragdoll Ragdoll { get; set; }
        private Timer DeleteTimer { get; set; }

        public void UpdateDeleteTimer()
        {
            if(Ragdoll==null) return;//noch nicht initialisiert
            if(timerDisposed) return;
            var timer = DeleteTimer;
            timer.Change(1000, Timeout.Infinite);
        }
    }
}