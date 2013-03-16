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
using Microsoft.Xna.Framework;

namespace DepthMapper
{

    public static class Extensions
    {
        public static Vector3 ToVector3(this Microsoft.Kinect.Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        #region Variablen

        #endregion

        #region Konstruktoren

        #endregion

        #region Eigenschaften

        #endregion

        #region Methoden

        #endregion
    }
}
