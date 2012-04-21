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
using KinectAddons;
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

        public TransferableJoint this[JointType val]
        {
            get { return Joints.Find(jnt => jnt.JointType == val); }
        }

        public List<TransferableJoint> Joints { get; set; }

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
    }
}
