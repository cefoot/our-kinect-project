using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KinectLetterParticelSwarm
{
    //fixed object that can activate function
    public class FixedObjects
    {
        //size
        double height;
        double radius;

        //position
        Vector3 pos;
        Vector4 rotation;

        //function
        //ObjectFunction objFunc;

        //is active?
        Boolean isActive;

        //intersects fixedObject with given point, simple sphere approximation
        public Boolean intersectsSphere(Vector3 point)
        {

            return Vector3.Distance(point,pos) < radius;
        }
        
    }
}
