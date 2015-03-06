using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace De.DataExperts.conhITApp
{
    public static class Extensions
    {

        /// <summary>
        /// Converts a coordinate from the cartesian coordinate system to the polar coordinate system.
        /// </summary>
        /// <param name="pnt"></param>
        /// <returns></returns>
        public static void ComputePolarCoordinate(this Point pnt, out double angle, out double radius)
        {
            radius = Math.Sqrt(pnt.X * pnt.X + pnt.Y * pnt.Y);
            angle = Math.Atan2(pnt.X, pnt.Y);
            angle *= -1;//Make rotation clockwise
            angle += Math.PI;//Make rotation start at top middle
            angle /= Math.PI;//..
            angle *= 180;    //turn to degree
        }

        public static Point GetControlPoint(this CameraSpacePoint camPoint, KinectSensor sensor, Size ctrlDimensions)
        {
            var colorPnt = sensor.CoordinateMapper.MapCameraPointToColorSpace(camPoint);
            var pnt = new Point();
            pnt.X = colorPnt.X / sensor.ColorFrameSource.FrameDescription.Width * ctrlDimensions.Width;
            pnt.Y = colorPnt.Y / sensor.ColorFrameSource.FrameDescription.Height * ctrlDimensions.Height;
            return pnt;
        }

        /// <summary>
        /// Converts a coordinate from the polar coordinate system to the cartesian coordinate system.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Point ComputeCartesianCoordinate(this double angle, double radius)
        {
            // convert to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
    }
}
