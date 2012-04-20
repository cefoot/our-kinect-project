﻿/* $HeadURL$
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
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Kinect;

namespace KinectAddons
{
    [Serializable]
    public class TransferableJoint
    {
        public JointType JointType { get; set; }
        public SkeletonPoint SkeletPoint { get; set; }
    }

    public static class SkeletonExtensions
    {

        public static void SerializeJointData(this List<TransferableJoint> transferableJoints, Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, transferableJoints);
            stream.Flush();
        }

        public static List<TransferableJoint> DeserializeJointData(this Byte[] sendedData)
        {
            var serializer = new XmlSerializer(typeof(List<TransferableJoint>));
            List<TransferableJoint> transferableJoint;
            using(var stream = new MemoryStream(sendedData))
            {
                transferableJoint = serializer.Deserialize(stream) as List<TransferableJoint>;
                stream.Close();
            }
            return transferableJoint;
        }

        public static List<TransferableJoint> CreateTransferable(this Skeleton skelet)
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

        public static List<TransferableJoint> DeserializeJointData(this Stream stream)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(stream) as List<TransferableJoint>;

        }
    }
}
