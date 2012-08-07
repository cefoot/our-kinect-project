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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using KinectServer.Properties;
using Microsoft.Kinect;

namespace KinectServer
{

    partial class Commands
    {
        public const byte ToLowConfidenceLevel = byte.MaxValue;
        public IList<TcpListener> AudioAngleServerListener { get; set; }

        public IList<TcpClient> ConnectedAudioAngleClients { get; set; }

        private void AudioAngleClientConnected(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            if(Stopped) return;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            tcpListener.BeginAcceptSocket(AudioAngleClientConnected, tcpListener);
            Console.WriteLine("AudioAngle Client Connected : '{0}'", tcpClient.Client.RemoteEndPoint);
            ConnectedAudioAngleClients.Add(tcpClient);
            _clientQueueBuffer[tcpClient] = Queue.Synchronized(new Queue());
            ThreadPool.QueueUserWorkItem(SendToAudioAngleSocket, tcpClient);
        }

        private void SendToAudioAngleSocket(object state)
        {
            var client = state as TcpClient;
            try
            {
                using (var networkStream = client.GetStream())
                {
                    while (client.Connected)
                    {
                        if (_clientQueueBuffer[client].Count == 0) continue;
                        var angle = (byte)_clientQueueBuffer[client].Dequeue();
                        networkStream.WriteByte(angle);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                ConnectedAudioAngleClients.Remove(client);
            }
        }

        private bool StartAudioAngle()
        {
            KinectAudio.SoundSourceAngleChanged += KinectAudioSoundSourceAngleChanged;
            Console.WriteLine(String.Concat("AudioAngle Server läuft auf Port:", Settings.Default.AudioAnlgeServerPort));
            return true;
        }

        void KinectAudioSoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            foreach (var connectedClient in ConnectedAudioAngleClients)
            {
                //um 90 grad verschieben das byte erst bei 0 losgeht
                _clientQueueBuffer[connectedClient].Enqueue(e.ConfidenceLevel > .4 ? (byte)(e.Angle + 90) : ToLowConfidenceLevel);
            }
        }
    }
}
