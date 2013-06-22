using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KinectLetterParticelSwarm
{
    //Letters move around and are attracted to Attractors, but not wise versa
    public class Attractor : Letter
    {
        public Attractor(int i)
        {
            Random r = new Random(i);
            posOld = new Vector3(((float)r.NextDouble() * 2) - 5.0f, ((float)r.NextDouble() * 2) - 5.0f, ((float)r.NextDouble() * 2) - 5.0f);
            posNew = new Vector3(posOld.X, posOld.Y, posOld.Z);
        }
	    Boolean active;
	    Boolean visible;

    }
}
