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
using System.Threading;
using KinectAddons;
using KinectServer.Properties;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.IO;

namespace KinectServer
{

    public partial class Commands
    {
        public KinectSensor Kinect { get; set; }

        public Commands(KinectSensor kinect,KinectAudioSource kinectAudioSource):this()
        {
            Kinect = kinect;
            KinectAudio = kinectAudioSource;
        }

        public Commands()
        {
            ServerListener = new List<TcpListener>();
            AudioServerListener = new List<TcpListener>();

            ConnectedClients = new List<TcpClient>();
            ConnectedAudioClients = new List<TcpClient>();

            var hostAddress = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddress in hostAddress.AddressList)
            {
                ServerListener.Add(new TcpListener(ipAddress, Settings.Default.ServerPort));
                AudioServerListener.Add(new TcpListener(ipAddress, Settings.Default.AudioServerPort));
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
                Console.WriteLine(connectedClient.Client.RemoteEndPoint.ToString());
            }
            return true;
        }

        [Description("Startet den Server")]
        public bool Start()
        {
            Kinect.SkeletonFrameReady += RuntimeSkeletonFrameReady;
            Console.WriteLine("Kinect gestartet");
            foreach (var tcpListener in ServerListener)
            {
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(SkeletClientConnected, tcpListener);
            }

            foreach (var tcpListener in AudioServerListener)
            {
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(AudioClientConnected, tcpListener);
            }

            Console.WriteLine(String.Concat("Server läuft auf Port:", Settings.Default.ServerPort));
            return StartAudio();
        }
    }
}
