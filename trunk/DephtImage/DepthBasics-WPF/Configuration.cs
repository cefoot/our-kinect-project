using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MathNet.Numerics.LinearAlgebra.Single;

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
        Plane secondBoard;
        Plane[] planes = new Plane[40];

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


                    this.background3D[this.twoDtoOneD(x, y)] = new Vector3(tempPoint.X, tempPoint.Y, tempPoint.Z);
                }
            }
            
        }

        internal void calculatePlanes()
        {
            Console.WriteLine(this.background3D.Length);
            for (int i = 0; i < planes.Length; i++)
            {
                Vector3[] points = new Vector3[3];
                for (int x = 0; x < points.Length; x++)
                {
                    points[x] = getRandomNotNullPoint();
                    //just to be sure...
                    if (points[x].Equals(new Vector3(0.0f, 0.0f, 0.0f)))
                    {
                        x = -1;
                        Console.WriteLine("find null");
                    }
                    else
                    {
                        Console.WriteLine(points[x]);
                    }
                                        
                }
                //now i have my 3 points, create plane
                planes[i] = new Plane(points[0],points[1],points[2]);
                Console.WriteLine(planes[i].D + " " + planes[i].Normal);
            }



            
        }
        Random r = new Random();
        public Vector3 getRandomNotNullPoint()
        {
            
            Vector3 point = new Vector3(0.0f,0.0f,0.0f);
            int iteration = 0;
            int maxIteration = 1000;
            while (point.Equals(new Vector3(0.0f, 0.0f, 0.0f)) && iteration<maxIteration)
            {
                int randIndex = r.Next(this.background3D.Length);
                point = this.background3D[randIndex];
                iteration++;
            }

            return point;
        }

        internal void calculatePlane_old()
        {

            //Vector3 point1 = this.background3D[this.twoDtoOneD(326,275)];
            //Vector3 point2 = this.background3D[this.twoDtoOneD(159,252)];
            //Vector3 point3 = this.background3D[this.twoDtoOneD(448,252)];
            Vector3 point1 = this.background3D[this.twoDtoOneD(40, 116)];
            Vector3 point2 = this.background3D[this.twoDtoOneD(500, 120)];
            Vector3 point3 = this.background3D[this.twoDtoOneD(340, 305)];
            Console.WriteLine(point1);
            Console.WriteLine(point2);
            Console.WriteLine(point3);

            this.board = new Plane(point1,point2,point3);

            Console.WriteLine(this.board.D);
            Console.WriteLine(this.board.Normal);

            point1 = this.background3D[this.twoDtoOneD(140, 250)];
            point2 = this.background3D[this.twoDtoOneD(400, 250)];
            point3 = this.background3D[this.twoDtoOneD(300, 170)];
            Console.WriteLine(point1);
            Console.WriteLine(point2);
            Console.WriteLine(point3);


            this.secondBoard = new Plane(point1, point2, point3);
            Console.WriteLine(this.secondBoard.D);
            Console.WriteLine(this.secondBoard.Normal );

        }

        public float ComputeDistanceToBoard(Vector3 point)
        {
            float value = 0;
            for (int i = 0; i < this.planes.Length; i++)
            {
                value += planes[i].DotCoordinate(point);
            }
           
            return (value/(float)this.planes.Length);
        }
    }
    /*
     * // Geometric Tools, LLC
// Copyright (c) 1998-2012
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt
//
// File Version: 5.0.1 (2010/10/01)

#include "Wm5MathematicsPCH.h"
#include "Wm5ApprPlaneFit3.h"
#include "Wm5EigenDecomposition.h"
#include "Wm5LinearSystem.h"

namespace Wm5
{
//----------------------------------------------------------------------------
template <typename Real>
bool HeightPlaneFit3 (int numPoints, const Vector3<Real>* points, Real& a,
    Real& b, Real& c)
{
    // You need at least three points to determine the plane.  Even so, if
    // the points are on a vertical plane, there is no least-squares fit in
    // the 'height' sense.  This will be trapped by the determinant of the
    // coefficient matrix.

    // Compute sums for linear system.
    Real sumX = (Real)0, sumY = (Real)0, sumZ = (Real)0.0;
    Real sumXX = (Real)0, sumXY = (Real)0, sumXZ = (Real)0;
    Real sumYY = (Real)0, sumYZ = (Real)0;
    int i;
    for (i = 0; i < numPoints; ++i)
    {
        sumX += points[i][0];
        sumY += points[i][1];
        sumZ += points[i][2];
        sumXX += points[i][0]*points[i][0];
        sumXY += points[i][0]*points[i][1];
        sumXZ += points[i][0]*points[i][2];
        sumYY += points[i][1]*points[i][1];
        sumYZ += points[i][1]*points[i][2];
    }

    Real A[3][3] =
    {
        {sumXX, sumXY, sumX},
        {sumXY, sumYY, sumY},
        {sumX,  sumY,  (Real)numPoints}
    };

    Real B[3] =
    {
        sumXZ,
        sumYZ,
        sumZ
    };

    Real X[3];

    bool nonsingular = LinearSystem<Real>().Solve3(A, B, X);
    if (nonsingular)
    {
        a = X[0];
        b = X[1];
        c = X[2];
    }
    else
    {
        a = Math<Real>::MAX_REAL;
        b = Math<Real>::MAX_REAL;
        c = Math<Real>::MAX_REAL;
    }

    return nonsingular;
}
//----------------------------------------------------------------------------
template <typename Real>
Plane3<Real> OrthogonalPlaneFit3 (int numPoints, const Vector3<Real>* points)
{
    // Compute the mean of the points.
    Vector3<Real> origin = Vector3<Real>::ZERO;
    int i;
    for (i = 0; i < numPoints; i++)
    {
        origin += points[i];
    }
    Real invNumPoints = ((Real)1)/numPoints;
    origin *= invNumPoints;

    // compute sums of products
    Real sumXX = (Real)0, sumXY = (Real)0, sumXZ = (Real)0;
    Real sumYY = (Real)0, sumYZ = (Real)0, sumZZ = (Real)0;
    for (i = 0; i < numPoints; ++i) 
    {
        Vector3<Real> diff = points[i] - origin;
        sumXX += diff[0]*diff[0];
        sumXY += diff[0]*diff[1];
        sumXZ += diff[0]*diff[2];
        sumYY += diff[1]*diff[1];
        sumYZ += diff[1]*diff[2];
        sumZZ += diff[2]*diff[2];
    }

    sumXX *= invNumPoints;
    sumXY *= invNumPoints;
    sumXZ *= invNumPoints;
    sumYY *= invNumPoints;
    sumYZ *= invNumPoints;
    sumZZ *= invNumPoints;

    // Setup the eigensolver.
    EigenDecomposition<Real> esystem(3);
    esystem(0,0) = sumXX;
    esystem(0,1) = sumXY;
    esystem(0,2) = sumXZ;
    esystem(1,0) = sumXY;
    esystem(1,1) = sumYY;
    esystem(1,2) = sumYZ;
    esystem(2,0) = sumXZ;
    esystem(2,1) = sumYZ;
    esystem(2,2) = sumZZ;

    // Compute eigenstuff, smallest eigenvalue is in last position.
    esystem.Solve(false);

    // Get plane normal.
    Vector3<Real> normal = esystem.GetEigenvector3(2);

    // The minimum energy.
    return Plane3<Real>(normal, origin);
}
//----------------------------------------------------------------------------

//----------------------------------------------------------------------------
// Explicit instantiation.
//----------------------------------------------------------------------------
template WM5_MATHEMATICS_ITEM
bool HeightPlaneFit3<float> (int, const Vector3<float>*, float&, float&,
    float&);

template WM5_MATHEMATICS_ITEM
Plane3<float> OrthogonalPlaneFit3<float> (int, const Vector3<float>*);

template WM5_MATHEMATICS_ITEM
bool HeightPlaneFit3<double> (int, const Vector3<double>*, double&, double&,
    double&);

template WM5_MATHEMATICS_ITEM
Plane3<double> OrthogonalPlaneFit3<double> (int, const Vector3<double>*);
//----------------------------------------------------------------------------
}

     * 
     * 
     */

}
