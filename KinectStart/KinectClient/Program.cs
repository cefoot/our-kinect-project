using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using KinectAddons;
using Microsoft.Kinect;

namespace KinectClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TcpClient();
            client.Connect("WS201736", 666);
            var stream = client.GetStream();
            while (client.Connected)
            {
                //var byt = stream.ReadByte();
                //if (byt < 0) continue;
                //Console.Write((char)byt);
                var jointDatas = stream.DeserializeJointData();
                foreach (var jointData in jointDatas)
                {
                    Console.WriteLine(jointData.SkeletPoint);
                }
               
            }
        }
    }
}
