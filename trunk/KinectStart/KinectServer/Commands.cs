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

        protected bool Stopped { get; set; }

        private readonly Dictionary<TcpClient, Queue> _clientQueueBuffer = new Dictionary<TcpClient, Queue>();

        public Commands(KinectSensor kinect)
            : this()
        {
            Kinect = kinect;
            var kinectAudioSource = kinect.AudioSource;
            KinectAudio = kinectAudioSource;
        }

        public Commands()
        {
            Stopped = true;
            ServerListener = new List<TcpListener>();
            AudioServerListener = new List<TcpListener>();
            AudioAngleServerListener = new List<TcpListener>();

            ConnectedSkeletClients = new List<TcpClient>();
            ConnectedAudioClients = new List<TcpClient>();
            ConnectedAudioAngleClients = new List<TcpClient>();

            var hostAddress = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddress in hostAddress.AddressList)
            {
                ServerListener.Add(new TcpListener(ipAddress, Settings.Default.ServerPort));
                AudioServerListener.Add(new TcpListener(ipAddress, Settings.Default.AudioServerPort));
                AudioAngleServerListener.Add(new TcpListener(ipAddress, Settings.Default.AudioAnlgeServerPort));
            }
        }

        [Description("Zeigt die Hilfe an")]
        public bool Help()
        {
            Console.Clear();
            foreach (var methodInfo in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
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

        [Description("Stoppt den Server")]
        public bool Stop()
        {
            Stopped = true;
            AudioServerListener.AsParallel().ForAll(listener => listener.Stop());
            ServerListener.AsParallel().ForAll(listener => listener.Stop());
            AudioAngleServerListener.AsParallel().ForAll(listener => listener.Stop());
            Kinect.SkeletonFrameReady -= RuntimeSkeletonFrameReady;
            Kinect.SkeletonStream.Disable();
            Kinect.AudioSource.Stop();
            Kinect.Stop();
            return true;
        }

        [Description("Beendet den Server")]
        public bool Exit()
        {
            if (!Stopped)
            {
                Stop();
            }
            return false;
        }

        [Description("Zeigt den Status an")]
        public bool Status()
        {
            Console.Clear();
            foreach (var connectedClient in ConnectedSkeletClients)
            {
                Console.WriteLine(connectedClient.Client.RemoteEndPoint.ToString());
            }
            return true;
        }

        [Description("Startet den Server")]
        public bool Start()
        {
            Kinect.Start();// alt Initialize(RuntimeOptions.UseSkeletalTracking);//was will ich haben
            Kinect.ElevationAngle = Settings.Default.KinectAngle;//neigung
            Kinect.SkeletonStream.Enable();
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

            foreach (var tcpListener in AudioAngleServerListener)
            {
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(AudioAngleClientConnected, tcpListener);
            }
            Stopped = false;
            var start = StartSkelet();
            start &= StartAudio();
            start &= StartAudioAngle();
            Console.WriteLine("Kinect gestartet");
            return start;
        }
    }
}
