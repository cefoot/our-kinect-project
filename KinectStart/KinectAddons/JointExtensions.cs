/* $HeadURL$
  ------------------------------------------------------------------------------
        (c) by data experts gmbh
              Postfach 1130
              Woldegker Str. 12
              17001 Neubrandenburg

  Dieses Dokument und die hierin enthaltenen Informationen unterliegen
  dem Urheberrecht und duerfen ohne die schriftliche Genehmigung des
  Herausgebers weder als ganzes noch in Teilen dupliziert oder reproduziert
  noch manipuliert werden.
*/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace Kinect
{

    public static class JointExtensions
    {

        public static Joint ScaleOwn(this Joint jnt, int width, int height)
        {
            var position = jnt.Position;
            var xVal = position.X + 1.0f;//damit nichtmehr negativ
            var yVal = position.Y * -1 + 1.0f;//umdrehen damit 0 oben
            var posX = ((float)width / 2) * xVal;
            var posY = ((float)height / 2) * yVal;
            jnt.Position = new SkeletonPoint{X=posX,Y=posY,Z = position.Z};
            return jnt;
        }


        public static Vector2 Convert(this SkeletonPoint point)
        {
            return new Vector2(point.X,point.Y);
        }
    }
}
