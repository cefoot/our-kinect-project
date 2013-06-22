using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KinectLetterParticelSwarm
{
    //simple Letters
    public class Letter
    {

        public Letter()
        {
        }
        public Letter(int i)
        {
            Random r = new Random(i);
            posOld = new Vector3(((float)r.NextDouble() * 5) - 10.0f, ((float)r.NextDouble() * 5) - 10.0f, ((float)r.NextDouble() * 5) - 10.0f);
            posNew = new Vector3(posOld.X, posOld.Y, posOld.Z);
        }
        //position
        public Vector3 posOld { get; set; }
        public Vector3 posNew { get; set; }
        public Vector4 rotation { get; set; }

        //velocity
        public Vector3 velOld { get; set; }
        public Vector3 velNew { get; set; }

        String content;

        //Position in assoziated word
        int posInWord;

        double size;

        //Word where letter is contained in
        Word assoziatedWord;

       
        private Vector3 calcNewPos(float stepSize, Vector3 accOld)
        {
            posNew = Vector3.Add(Vector3.Add(posOld, Vector3.Multiply(velOld, (float)stepSize)), Vector3.Multiply(accOld, 0.5f * (float)Math.Pow(stepSize, 2)));
            return posNew;

        }

    }
}
