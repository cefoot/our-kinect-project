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
            client.Connect("LAPTOP", 667);
            var stream = client.GetStream();
            while (client.Connected)
            {
                var readByte = stream.ReadByte();
                if(readByte < 0) continue;
                Console.Write(readByte);
            }
        }
    }
}
