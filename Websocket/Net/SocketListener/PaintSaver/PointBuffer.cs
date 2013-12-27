using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using PaintSaver;

namespace SocketListener
{
    class PointBuffer
    {
        int count = 0;
        int minElements = 1;
        float curX = 1;
        float curY = 1;
        float curZ = 1;
        bool ready = false;
        Queue<float> lastX;
        Queue<float> lastY;
        Queue<float> lastZ;
        int currentElementPointer = 0;
        String name = "";
        public PointBuffer(int min,String name)
        {
            minElements = min;
            lastX = new Queue<float>(minElements);
            lastY = new Queue<float>(minElements);
            lastZ = new Queue<float>(minElements);
            this.name = name;
        }

        public void setMinElements(int min)
        {
            this.minElements = min;
        }



        public SkeletonPoint getCurrentPoint(float scale)
        {

            SkeletonPoint currentPoint = new SkeletonPoint();
            currentPoint.X = curX * scale;
            currentPoint.Y = curY * scale;
            currentPoint.Z = curZ * scale;
            return currentPoint;

        }
        public SkeletonPoint add(SkeletonPoint newPoint)
        {
            return this.add(newPoint, 1f);
        }

        public SkeletonPoint add(SkeletonPoint newPoint,float scale)
        {
            lastX.Enqueue(newPoint.X);
            lastY.Enqueue(newPoint.Y);
            lastZ.Enqueue(newPoint.Z);

            if (!ready)
            {

                count++;
                if (count >= minElements)
                {
                    ready = true;
                    Console.WriteLine(name+" ready");
                }
            }
            else
            {
                lastX.Dequeue();
                lastY.Dequeue();
                lastZ.Dequeue();
            }
            CalcNewPoint();

            return getCurrentPoint(scale);
        }

        private void CalcNewPoint()
        {
            for (var i = 0; i < lastX.Count; i++)
            {
                curX = lastX.Average();
                curY = lastY.Average();
                curZ = lastZ.Average();
            }
        }

        public void ResetBuffer()
        {
            lastX.Clear();
            lastY.Clear();
            lastZ.Clear();

            curX = 1;
            curY = 1;
            curZ = 1;

            ready = false;

            count = 0;
        }

        public bool IsBufferReady()
        {
            return ready;
        }

    }
}
