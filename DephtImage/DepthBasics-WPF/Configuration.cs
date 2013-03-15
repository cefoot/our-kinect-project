using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Microsoft.Samples.Kinect.DepthBasics
{
    //calculates and holds all information where the interaction might happen
    class Configuration
    {
        //original image size
        int imageWidth;
        int imageHeight;

        //position of kinect relativ to board
        //above = 1, under = -1, left = -0.5, right = 0.5
        int kinectPos = 1;
       

        private float validationThreshold;

        float[] background;
        float[] validationArray;

        Vector3[] background3D;
        Plane board;

        public Configuration(int width, int height, short[] depthImage)
        {
            //set all necessary value
            this.imageWidth = width;
            this.imageHeight = height;

            this.background = new float[width * height];
            background3D = new Vector3[width * height];
            this.validationArray = new float[height*height];
            Array.Clear(background,0,background.Length);
            Array.Clear(validationArray, 0, validationArray.Length);

        }

        public float[] addToBackground(short[] depthImage)
        {
            for (int i = 0; i < depthImage.Length; i++)
            {
                this.background[i] += (ushort)(depthImage[i] >> Microsoft.Kinect.DepthImageFrame.PlayerIndexBitmaskWidth);
                this.background[i] /= 2.0f;
                                
            }
            return this.background;

        }

        public float[] getBackgroundImage()
        {
            return this.background;
        }

        public float getBackgroundAt(int x, int y)
        {
            return this.background[this.twoDtoOneD(x,y)];
        }

        public float getBackgroundAt(int pos)
        {
            return this.background[pos];
        }

        public Vector3 getBackground3DAt(int pos)
        {
            return this.background3D[pos];
        }

        public int twoDtoOneD(int x, int y)
        {
            return (y * this.imageWidth + x);
        }

        public int[] substractImageFromBackground(int[] depthImage)
        {
            for (int i = 0; i < depthImage.Length; i++)
            {
                depthImage[i] = (int)this.background[i] - depthImage[i];
            }
            return depthImage;
        }

        public void createValidationArray(float threshold)
        {
            this.validationThreshold = threshold;
                        
            for (int yP = 0; yP < imageHeight; ++yP)
            {
                for (int  yB= 0; yB < imageHeight; ++yB)
                {
                    //alle möglichen Werte von yP und yB
                    this.validationArray[yP+yB*imageHeight] = threshold - Math.Abs(yP-yB);
                }
            }

        }

        public Tuple<int, float>[] getPositionsOverThreshold(int[] depthImage, float threshold)
        {
            List<Tuple<int, float>> posValue = new List<Tuple<int, float>>();
            for (int i = 0; i < depthImage.Length; i++)
            {
                float value = this.background[i] - (float)depthImage[i];
                if (depthImage[i] < 8000 && value > threshold)
                {

                    posValue.Add(new Tuple<int, float>(i, (float)depthImage[i]));

                }
            }
            return posValue.ToArray();
        }



        internal void create3DBackgorund(Microsoft.Kinect.CoordinateMapper coordMapper,Microsoft.Kinect.DepthImageFormat imageFormat)
        {

            Microsoft.Kinect.DepthImagePoint currentPoint = new Microsoft.Kinect.DepthImagePoint();
            for(int x=0;x<this.imageWidth;x++)
            {
                for(int y=0;y<imageHeight;y++)
                {
                    //Warning, float gets cut off
                    currentPoint.Depth = (int)Math.Round(this.getBackgroundAt(x,y));
                    currentPoint.X = x;
                    currentPoint.Y = y;
                    Microsoft.Kinect.SkeletonPoint tempPoint = coordMapper.MapDepthPointToSkeletonPoint(imageFormat, currentPoint);


                    this.background3D[this.twoDtoOneD(x, y)] = new Vector3(tempPoint.X, tempPoint.Y, tempPoint.Z/100.0f);
                }
            }
            
        }

        internal void calculatePlane()
        {

            Vector3 point1 = this.background3D[this.twoDtoOneD(326,275)];
            Vector3 point2 = this.background3D[this.twoDtoOneD(159,252)];
            Vector3 point3 = this.background3D[this.twoDtoOneD(448,252)];

            this.board = new Plane(point1,point2,point3);

        }

        public float ComputeDistanceToBoard(Vector3 point)
        {
            float dot = Vector3.Dot(this.board.Normal, point);
            float value = dot - this.board.D;
            return value;
        }
    }
}
