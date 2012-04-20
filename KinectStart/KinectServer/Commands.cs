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
using System.Reflection;
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

        public IList<TcpClient> ConnectedClients { get; set; }

        public Commands(KinectSensor kinect):this()
        {
            Kinect = kinect;
        }

        public Commands()
        {
            ServerListener = new List<TcpListener>();
            ConnectedClients = new List<TcpClient>();
            var hostAddress = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddress in hostAddress.AddressList)
            {
                ServerListener.Add(new TcpListener(ipAddress, Settings.Default.ServerPort));
            }
        }

        [Description("Zeigt die Hilfe an")]
        public bool Help()
        {
            Console.Clear();
            foreach (var methodInfo in GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.DeclaredOnly))
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

        [Description("Sendet eine Leere Nachricht an Alle Clients")]
        public bool Dummy()
        {
            ConnectedClients.AsParallel().ForAll(socket => SendToSocket(socket, new Skeleton().CreateTransferable()));
            return true;
        }

        [Description("Beendet den Server")]
        public bool Stop()
        {
            Kinect.SkeletonFrameReady -= RuntimeSkeletonFrameReady;
            Kinect.Stop();
            return false;
        }

        [Description("Zeigt den Status an")]
        public bool Status()
        {
            Console.Clear();
            foreach (var connectedClient in ConnectedClients)
            {
                Console.WriteLine(connectedClient.Client.RemoteEndPoint.AddressFamily);
            }
            return true;
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
                tcpListener.BeginAcceptTcpClient(ClientConnected, tcpListener);
            }
            Console.WriteLine(String.Concat("Server läuft auf Port:", Settings.Default.ServerPort));
            return true;
        }

        private void ClientConnected(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            Console.WriteLine("Client Connected : '{0}'", tcpClient);
            ConnectedClients.Add(tcpClient);
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

        private static void SendToSocket(TcpClient tcpClient, List<TransferableJoint> transferableJoints)
        {
            try
            {
                if (!tcpClient.Connected) return;

                transferableJoints.SerializeJointData(tcpClient.GetStream());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
