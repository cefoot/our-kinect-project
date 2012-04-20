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

namespace StickmansSpeech
{

    public class Stickman
    {
        #region Variablen

        #endregion

        #region Konstruktoren

        public Stickman()
        {
            BoneColor = Color.Black;
        }

        #endregion

        #region Eigenschaften

        public Skeleton Skeleton { get; set; }

        private bool _isSpeaker;
        public bool IsSpeaker
        {
            get { return _isSpeaker; }
            set
            {
                _isSpeaker = value;
                BoneColor = value ? Color.Red : Color.Black;
            }
        }

        public Color BoneColor { get; set; }

        #endregion

        #region Methoden

        // override object.Equals
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return ((Stickman) obj).Skeleton.TrackingId.Equals(Skeleton.TrackingId);
            
        }

// override object.GetHashCode
        public override int GetHashCode()
        {
            return Skeleton.GetHashCode();
        }
        #endregion
    }
}
