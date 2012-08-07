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
using System.Threading.Tasks;
using KinectAddons;
using KinectServer.Properties;
using Microsoft.Kinect;

namespace KinectServer
{

    partial class Commands
    {

        private IList<TcpListener> DepthServerListener { get; set; }

        private IList<TcpClient> ConnectedDepthClients { get; set; }

        private void DepthClientConnected(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            if (Stopped) return;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            tcpListener.BeginAcceptSocket(DepthClientConnected, tcpListener);
            tcpClient.NoDelay = true;
            Console.WriteLine("Client Connected : '{0}'", tcpClient.Client.RemoteEndPoint);
            ConnectedDepthClients.Add(tcpClient);
            _clientQueueBuffer[tcpClient] = Queue.Synchronized(new Queue());
            ThreadPool.QueueUserWorkItem(SendToDepthSocket, tcpClient);
        }

        private void SendToDepthSocket(object state)
        {
            var client = state as TcpClient;
            try
            {
                using (var networkStream = new BufferedStream(client.GetStream()))
                {
                    while (client.Connected)
                    {
                        if (_clientQueueBuffer[client].Count == 0) continue;
                        var depths = _clientQueueBuffer[client].Dequeue() as short[];
                        depths.SerializeDepthData(networkStream);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                ConnectedDepthClients.Remove(client);
            }
        }

        private void RuntimeDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            WorkOnDepthFrame(e.OpenDepthImageFrame());
        }

        private void WorkOnDepthFrame(DepthImageFrame openDepthFrame)
        {
            using (openDepthFrame)
            {
                if (openDepthFrame == null)
                {
                    return;
                }
                if(ConnectedDepthClients.Count == 0) return;

                var depthsData = new short[openDepthFrame.PixelDataLength];
                openDepthFrame.CopyPixelDataTo(depthsData);
                foreach (var connectedClient in ConnectedDepthClients)
                {
                    _clientQueueBuffer[connectedClient].Enqueue(depthsData);
                }
            }
        }

        private void DepthFrameResolving(object state)
        {
            var depthStream = state as DepthImageStream;
            while (!Stopped)
            {
                var nextDepthFrame = depthStream.OpenNextFrame(Settings.Default.RefreshRate);
                WorkOnDepthFrame(nextDepthFrame);
            }
        }

        private bool StartDepth()
        {
            Console.WriteLine(String.Concat("Depth Server läuft auf Port:", Settings.Default.DepthServerPort));
            //Kinect.DepthFrameReady += RuntimeDepthFrameReady;
            return ThreadPool.QueueUserWorkItem(DepthFrameResolving, Kinect.DepthStream);
        }
    }
}
