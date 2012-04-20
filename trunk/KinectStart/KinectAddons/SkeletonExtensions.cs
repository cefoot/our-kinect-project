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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Kinect;

namespace KinectAddons
{

    public class TransferableJoint
    {
        public JointType JointType { get; set; }
        public SkeletonPoint SkeletPoint { get; set; }
    }

    public static class SkeletonExtensions
    {
        public static Byte[] SerializeJointData(this IList<TransferableJoint> transferableJoints)
        {
            var serializer = new XmlSerializer(transferableJoints.GetType());
            byte[] data;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, transferableJoints);
                stream.Flush();
                data = stream.GetBuffer();
                stream.Close();
            }
            return data;
        }

        public static IList<TransferableJoint> DeserializeJointData(this Byte[] sendedData)
        {
            var serializer = new XmlSerializer(typeof(IList<TransferableJoint>));
            IList<TransferableJoint> transferableJoint;
            using(var stream = new MemoryStream(sendedData))
            {
                transferableJoint = serializer.Deserialize(stream) as IList<TransferableJoint>;
                stream.Close();
            }
            return transferableJoint;
        }

        public static IList<TransferableJoint> CreateTransferable(this Skeleton skelet)
        {
            var joints = new List<TransferableJoint>();
            foreach (Joint joint in skelet.Joints)
            {
                joints.Add(new TransferableJoint
                               {
                                   JointType = joint.JointType,
                                   SkeletPoint = joint.Position
                               });
            }
            return joints;
        }
    }
}
