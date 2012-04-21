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
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.IO;

namespace KinectServer
{

    public class Commands
    {
        public KinectSensor Kinect { get; set; }

        public KinectAudioSource KinectAudio { get; set; }

        public IList<TcpListener> ServerListener { get; set; }

        public IList<TcpListener> AudioServerListener { get; set; }

        public IList<TcpClient> ConnectedClients { get; set; }
        
        public IList<TcpClient> ConnectedAudioClients { get; set; }

        private SpeechRecognitionEngine speechEngine;

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
                tcpListener.BeginAcceptTcpClient(ClientConnected, tcpListener);
            }

            foreach (var tcpListener in AudioServerListener)
            {
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(AudioClientConnected, tcpListener);
            }

            Console.WriteLine(String.Concat("Server läuft auf Port:", Settings.Default.ServerPort));
            return StartAudio();
        }

        private bool StartAudio()
        {
            KinectAudio.AutomaticGainControlEnabled = false;
            KinectAudio.EchoCancellationMode = EchoCancellationMode.None;
            KinectAudio.NoiseSuppression = false;
            
            var stream = KinectAudio.Start();
            Console.WriteLine("AudioKinect gestartet");

            var buffer = new byte[256];
            stream.BeginRead(buffer, 0, buffer.Length, AudioStreamBufferd, new Tuple<Stream,byte[]>(stream,buffer));

            Console.WriteLine(String.Concat("Audio Server läuft auf Port:", Settings.Default.AudioServerPort));
            return true;
        }

        public void AudioStreamBufferd(IAsyncResult asynResult)
        {
            var data = asynResult.AsyncState as Tuple<Stream, byte[]>;
            var count = data.Item1.EndRead(asynResult);
            var buffer = new byte[256];
            data.Item1.BeginRead(buffer, 0, buffer.Length, AudioStreamBufferd, new Tuple<Stream, byte[]>(data.Item1, buffer));
            
            foreach (var item in ConnectedAudioClients.AsParallel())
            {
                if(!item.Connected) continue;
                item.GetStream().Write(data.Item2, 0, count);
            }
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private void ClientConnected(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            Console.WriteLine("Client Connected : '{0}'", tcpClient.Client.LocalEndPoint.ToString());
            ConnectedClients.Add(tcpClient);
            tcpListener.BeginAcceptSocket(ClientConnected, tcpListener);

        }

        private void AudioClientConnected(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            Console.WriteLine("Audio Client Connected : '{0}'", tcpClient.Client.LocalEndPoint.ToString());
            ConnectedAudioClients.Add(tcpClient);
            tcpListener.BeginAcceptSocket(AudioClientConnected, tcpListener);

        }

        private void RuntimeSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var openSkeletonFrame = e.OpenSkeletonFrame())
            {

                if (openSkeletonFrame == null)
                {
                    return;
                }

                var skeletsData = new Skeleton[openSkeletonFrame.SkeletonArrayLength];
                openSkeletonFrame.CopySkeletonDataTo(skeletsData);
                var trackedSkeletons = skeletsData.Where(skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked);
                ConnectedClients.AsParallel().ForAll(socket => SendToSocket(socket, trackedSkeletons));
            }
        }

        private static void SendToSocket(TcpClient tcpClient, IEnumerable<Skeleton> trackedSkeletons)
        {
            try
            {
                if (!tcpClient.Connected) return;
                var trackedSkelets = new TrackedSkelletons();
                trackedSkelets.Skelletons = new List<List<TransferableJoint>>();
                bool found = false;
                foreach (var trackedSkelet in trackedSkeletons.AsParallel())
                {
                    trackedSkelets.Skelletons.Add(trackedSkelet.CreateTransferable());
                    found = true;
                }
                if (!found) return;
                trackedSkelets.SerializeJointData(tcpClient.GetStream());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
