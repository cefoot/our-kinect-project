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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using KinectAddons;
using KinectServer.Properties;
using Microsoft.Kinect;

namespace KinectServer
{

    public class Commands
    {
        public KinectSensor Kinect { get; set; }

        public IList<TcpListener> ServerListener { get; set; }

        public IList<Socket> ConnectedClients { get; set; }

        public Commands(KinectSensor kinect)
        {
            Kinect = kinect;
            var hostAddress = Dns.GetHostEntry(Dns.GetHostName());
            ServerListener = new List<TcpListener>();
            ConnectedClients = new List<Socket>();
            foreach (var ipAddress in hostAddress.AddressList)
            {
                ServerListener.Add(new TcpListener(ipAddress, Settings.Default.ServerPort));
            }
        }

        public Commands()
        {
        }

        [Description("Zeigt die Hilfe an")]
        public bool Help()
        {
            Console.Clear();
            foreach (var methodInfo in GetType().GetMethods())
            {
                if (methodInfo.GetParameters().Length > 0 || methodInfo.Name.Contains("_"))
                {
                    continue;
                }
                var descriptionAttribute = methodInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute;
                Console.WriteLine(String.Concat(methodInfo.Name, ":"));
                Console.WriteLine("  " + descriptionAttribute.Description);
            }
            return true;
        }

        [Description("Beendet den Server")]
        public bool Stop()
        {
            Kinect.SkeletonFrameReady -= RuntimeSkeletonFrameReady;
            Kinect.Stop();
            return false;
        }

        [Description("Startet den Server")]
        public bool Start()
        {
            Kinect.SkeletonStream.OpenNextFrame(Settings.Default.RefreshRate);
            Kinect.SkeletonFrameReady += RuntimeSkeletonFrameReady;
            Console.WriteLine("Kinect gestartet");
            foreach (var tcpListener in ServerListener)
            {
                tcpListener.Start();
                tcpListener.BeginAcceptSocket(ClientConnected, tcpListener);
            }
            Console.WriteLine(String.Concat("Server läuft auf Port:", Settings.Default.ServerPort));
            return true;
        }

        private void ClientConnected(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            ConnectedClients.Add(tcpListener.EndAcceptSocket(ar));
            tcpListener.BeginAcceptSocket(ClientConnected, tcpListener);

        }

        private void RuntimeSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var openSkeletonFrame = e.OpenSkeletonFrame();

            if (openSkeletonFrame == null)
            {
                return;
            }

            var skeletsData = new Skeleton[openSkeletonFrame.SkeletonArrayLength];
            openSkeletonFrame.CopySkeletonDataTo(skeletsData);

            foreach (var selectedStickman in skeletsData.Where(skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked).AsParallel())
            {
                ConnectedClients.AsParallel().ForAll(socket => SendToSocket(socket, selectedStickman.CreateTransferable()));
            }
        }

        private static void SendToSocket(Socket socket, IList<TransferableJoint> transferableJoints)
        {
            if (!socket.Connected) return;

            socket.Send(transferableJoints.CreateSendableData());
        }
    }
}
