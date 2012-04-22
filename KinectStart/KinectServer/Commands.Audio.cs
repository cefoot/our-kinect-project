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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using KinectServer.Properties;
using Microsoft.Kinect;

namespace KinectServer
{

    partial class Commands
    {
        public IList<TcpListener> AudioServerListener { get; set; }

        public KinectAudioSource KinectAudio { get; set; }

        public IList<TcpClient> ConnectedAudioClients { get; set; }

        private bool StartAudio()
        {
            KinectAudio.AutomaticGainControlEnabled = false;
            KinectAudio.EchoCancellationMode = EchoCancellationMode.None;
            KinectAudio.NoiseSuppression = false;

            var stream = KinectAudio.Start();
            Console.WriteLine("AudioKinect gestartet");

            var buffer = new byte[256];
            stream.BeginRead(buffer, 0, buffer.Length, AudioStreamBufferd, new Tuple<Stream, byte[]>(stream, buffer));

            Console.WriteLine(String.Concat("Audio Server läuft auf Port:", Settings.Default.AudioServerPort));
            return true;
        }

        public void AudioStreamBufferd(IAsyncResult asynResult)
        {
            var data = asynResult.AsyncState as Tuple<Stream, byte[]>;
            var count = data.Item1.EndRead(asynResult);
            var buffer = new byte[256];
            data.Item1.BeginRead(buffer, 0, buffer.Length, AudioStreamBufferd, new Tuple<Stream, byte[]>(data.Item1, buffer));

            TcpClient toRemove = null;
            foreach (var item in ConnectedAudioClients.AsParallel())
            {
                if (!item.Connected)
                {
                    toRemove = item;
                    continue;
                }
                item.GetStream().Write(data.Item2, 0, count);
            }
            ConnectedAudioClients.Remove(toRemove);
        }

        private void AudioClientConnected(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            if(Stopped) return;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            Console.WriteLine("Audio Client Connected : '{0}'", tcpClient.Client.RemoteEndPoint);
            ConnectedAudioClients.Add(tcpClient);
            tcpListener.BeginAcceptSocket(AudioClientConnected, tcpListener);

        }
    }
}
