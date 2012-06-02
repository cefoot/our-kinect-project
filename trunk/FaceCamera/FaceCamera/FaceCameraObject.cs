using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;

namespace FaceCamera
{
    public class FaceCameraObject
    {

        private KinectSensor kinectSensor;

        public void Initialize()
        {
            kinectSensor = KinectSensor.KinectSensors[0];
            kinectSensor.SkeletonStream.Enable();
        }

        public void StartKinect()
        {
            kinectSensor.Start();
        }

        public Vector3 GetNewCameraPosition(Vector3 nullPosition, Vector3 scaleVecStart, Vector3 scaleVecEnd)
        {
            var skeletFrame = kinectSensor.SkeletonStream.OpenNextFrame(1);

            if (skeletFrame == null) return nullPosition;
            var skeletsData = new Skeleton[skeletFrame.SkeletonArrayLength];
            skeletFrame.CopySkeletonDataTo(skeletsData);

            var trackedSkelets = skeletsData.ToList().Where(skelet => skelet.TrackingState == SkeletonTrackingState.Tracked);
            if (trackedSkelets.Count() < 1) return nullPosition;

            var skeletFirst = trackedSkelets.First();
            Vector3 headPosition = ScaleOwn(skeletFirst.Joints[JointType.Head], scaleVecStart, scaleVecEnd);


            return headPosition + nullPosition;
        }

        public static Vector3 ScaleOwn(Joint jnt, Vector3 scaleStart, Vector3 scaleEnd)
        {
            var position = jnt.Position;
            float widthStart = scaleStart.X;
            float heightStart = scaleStart.Y;
            float depthStart = scaleStart.Z;

            float widthEnd = scaleEnd.X;
            float heightEnd = scaleEnd.Y;
            float depthEnd = scaleEnd.Z;

            float widthRange = widthEnd - widthStart;
            float heightRange = heightEnd - heightStart;
            float depthRange = depthEnd - depthStart;

            float posX = (widthRange/2) * (position.X);
            float posY = (heightRange/2) * (position.Y);
            float posZ = (depthRange/2) * (position.Z);
            return new Vector3(posX, posY, posZ);

        }



    }
}
