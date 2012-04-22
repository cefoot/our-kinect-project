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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using KinectAddons;
using KinectServer.Properties;
using Microsoft.Kinect;

namespace KinectServer
{

    partial class Commands
    {
        private TcpListener _httpListener;

        public bool StartHttp()
        {
            foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                _httpListener = new TcpListener(address, Settings.Default.HttpPort);
                _httpListener.Start();
                _httpListener.BeginAcceptTcpClient(HttpReqeust, _httpListener);
            }
            return true;
        }

        private void HttpReqeust(IAsyncResult ar)
        {
            try
            {
                ProcessRequest(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ProcessRequest(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            if(Stopped) return;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);
            tcpListener.BeginAcceptTcpClient(HttpReqeust, tcpListener);
            string line;
            using (var reader = new StreamReader(tcpClient.GetStream()))
            {
                do
                {
                    line = reader.ReadLine();
                    Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + "::" + line);
                } while (!line.StartsWith("GET"));
                var tmp = reader.ReadLine();
                Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + "::" + tmp);
                while (tmp.Length > 0)
                {
                    Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + "::" + (tmp = reader.ReadLine()));
                }
                var urlSegments = line.Substring(0, line.IndexOf("HTTP")).Split('/');
                if (urlSegments.Length > 2)
                {
                    ProcessRequest(tcpClient, urlSegments);
                }
                else if (urlSegments.Length == 2)
                {
                    LoadResource(tcpClient, urlSegments);
                }
                else
                {
                    LoadResource(tcpClient, "", "index.html");
                }
            }
        }

        private void LoadResource(TcpClient tcpClient, params string[] urlSegments)
        {
            var resource = urlSegments[1];
            Debug.WriteLine("Lade Resource: " + resource);
            using (var writer = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8))
            {
                var name = resource.Split('.')[0];
                if(String.IsNullOrWhiteSpace(name))
                {
                    name = "index";
                }
                switch (name)
                {
                    case "index":
                    case "wz_jsgraphics":
                        var s = Resources.ResourceManager.GetString(name, Resources.Culture);
                        AddHeader(writer, name, s.Length);
                        //writer.WriteLine(Encoding.UTF8.GetByteCount(s));
                        writer.Write(s);
                        break;
                    case "favico":
                        var tmp = Resources.FOLDER;
                        AddHeader(writer, name, tmp.Length);
                        //writer.WriteLine(Encoding.UTF8.GetByteCount(tmp));
                        writer.Write(tmp);
                        break;
                }
            }
        }

        private void AddHeader(TextWriter writer, string resource, int length)
        {
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine("Server: KinectServer");
            //writer.WriteLine("Connection: Keep-Alive");
            writer.WriteLine("Connection: close");
            writer.Write("Content-Type: ");
            switch (resource)
            {
                case "wz_jsgraphics":
                case "wz_jsgraphics.js":
                    writer.Write("text/javascript");
                    break;
                case "index":
                case "index.html":
                    writer.Write("text/html");
                    break;
                case "data":
                    writer.Write("text/text");
                    break;
            }
            writer.WriteLine("; charset=UTF-8");
            writer.WriteLine("Content-Length: " + length);
            writer.WriteLine();
        }

        private void ProcessRequest(TcpClient client, String[] urlSegments)
        {
            FillResponse(client, GetRequestData(urlSegments));
        }

        private String GetRequestData(string[] urlSegments)
        {
            switch (urlSegments[1].TrimEnd('/').ToLower())
            {
                case "skelet":
                    return GetSkeletData(urlSegments);
                case "skelets":
                    return GetSkeletsData();
            }
            return "path not known";
        }

        private string GetSkeletsData()
        {
            var builder = new StringBuilder();
            var nextSkeletonFrame = Kinect.SkeletonStream.OpenNextFrame(Settings.Default.RefreshRate);
            if (nextSkeletonFrame == null)
            {
                return "no skelets tracked";
            }
            using (nextSkeletonFrame)
            {
                var skeletsData = new Skeleton[nextSkeletonFrame.SkeletonArrayLength];
                nextSkeletonFrame.CopySkeletonDataTo(skeletsData);
                var trackedSkeletons = skeletsData.Where(skeleton => skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked).ToList();
                if (trackedSkeletons.Count == 0) return "no skelets tracked";
                var trackedSkeleton = trackedSkeletons[0];
                AddJointData(trackedSkeleton, builder, JointType.Head);
                builder.Append(',');
                AddJointData(trackedSkeleton, builder, JointType.HipCenter);
                builder.Append(',');
                AddJointData(trackedSkeleton, builder, JointType.HandLeft);
                builder.Append(',');
                AddJointData(trackedSkeleton, builder, JointType.HandRight);
                builder.Append(',');
                AddJointData(trackedSkeleton, builder, JointType.FootLeft);
                builder.Append(',');
                AddJointData(trackedSkeleton, builder, JointType.FootRight);
                builder.Append(',');
                AddJointData(trackedSkeleton, builder, JointType.ShoulderCenter);
            }
            return builder.ToString();
        }

        private void AddJointData(Skeleton trackedSkeleton, StringBuilder builder, JointType jnt)
        {
            var joint = trackedSkeleton.Joints[jnt];
            var scaleOwn = joint.ScaleOwn(640, 480);
            builder.Append(((int)scaleOwn.Position.X).ToString());
            builder.Append(',');
            builder.Append(((int)scaleOwn.Position.Y).ToString());
        }

        private string GetSkeletData(IList<string> urlSegments)
        {
            var joint = urlSegments[2].TrimEnd('/');

            JointType jointType;
            if (!Enum.TryParse(joint, true, out jointType)) return "joint Not Found";
            var nextSkeletonFrame = Kinect.SkeletonStream.OpenNextFrame(10);
            using (nextSkeletonFrame)
            {
                if (nextSkeletonFrame == null)
                {
                    return "";
                }

                var skeletsData = new Skeleton[nextSkeletonFrame.SkeletonArrayLength];
                nextSkeletonFrame.CopySkeletonDataTo(skeletsData);
                var trackedSkeletons = skeletsData.Where(skeleton => skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked).ToList();
                if (trackedSkeletons.Count == 0) return "no skelets tracked";
                var desiredJoint = trackedSkeletons[0].Joints[jointType].ScaleOwn(640, 480);
                switch (urlSegments[3].TrimEnd('/').ToLower())
                {
                    case "x":
                        return desiredJoint.Position.X.ToString();
                    case "y":
                        return desiredJoint.Position.Y.ToString();
                    case "z":
                        return desiredJoint.Position.Z.ToString();
                }
                return "";
            }
        }

        private void FillResponse(TcpClient client, String requestData)
        {
            using (var writer = new StreamWriter(client.GetStream(), Encoding.UTF8))
            {
                AddHeader(writer, "data", requestData.Length);
                writer.Write(requestData);
            }
        }
    }
}
