﻿/* $HeadURL$
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
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KinectAddons;
using Microsoft.Kinect;

namespace KinectServer
{

    partial class Commands
    {

        private IList<TcpListener> ServerListener { get; set; }

        private IList<TcpClient> ConnectedClients { get; set; }

        private Dictionary<TcpClient, Queue> SendQueue = new Dictionary<TcpClient, Queue>();

        private void SkeletClientConnected(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            tcpListener.BeginAcceptSocket(SkeletClientConnected, tcpListener);
            tcpClient.NoDelay = true;
            Console.WriteLine("Client Connected : '{0}'", tcpClient.Client.LocalEndPoint);
            ConnectedClients.Add(tcpClient);
            SendQueue[tcpClient] = Queue.Synchronized(new Queue());
            ThreadPool.QueueUserWorkItem(SendToSocket, tcpClient);
        }

        private void SendToSocket(object state)
        {

            var dateTime = DateTime.Now;
            Debug.Print("Start Socket Send:" + dateTime.ToLongTimeString() + "." + dateTime.Millisecond);
            var client = state as TcpClient;
            try
            {
                while (client.Connected)
                {
                    if (SendQueue[client].Count == 0) continue;
                    var skelets = SendQueue[client].Dequeue() as IEnumerable<Skeleton>;
                    dateTime = DateTime.Now;
                    Debug.Print("Dequed:" + dateTime.ToLongTimeString() + "." + dateTime.Millisecond);
                    var trackedSkelets = new TrackedSkelletons
                    {
                        Skelletons = new List<List<TransferableJoint>>()
                    };
                    foreach (var trackedSkelet in skelets)
                    {
                        trackedSkelets.Skelletons.Add(trackedSkelet.CreateTransferable());
                        dateTime = DateTime.Now;
                        Debug.Print("Found!:" + dateTime.ToLongTimeString() + "." + dateTime.Millisecond);
                    }
                    trackedSkelets.SerializeJointData(client.GetStream());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                dateTime = DateTime.Now;
                Debug.Print("Ready Socke Send:" + dateTime.ToLongTimeString() + "." + dateTime.Millisecond);
            }
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
                var trackedSkeletons = skeletsData.Where(skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked).ToList();
                if(trackedSkeletons.Count == 0) return;
                foreach (var connectedClient in ConnectedClients)
                {
                    SendQueue[connectedClient].Enqueue(trackedSkeletons);

                    Debug.Print("Enqueued");
                }
            }
        }
    }
}
