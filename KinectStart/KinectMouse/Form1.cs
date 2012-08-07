using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using KinectAddons;

namespace KinectMouse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeKinect();
        }

        private void MoveMouseWithKinect()
        {
            throw new NotImplementedException();
        }

        private void InitializeKinect()
        {
            var skeletClient = new TcpClient
            {
                NoDelay = true
            };
            skeletClient.BeginConnect("WS201736", 666, ServerSkeletConnected, skeletClient);
        }

        private void ServerSkeletConnected(IAsyncResult ar)
        {
            var tcpClient = ar.AsyncState as TcpClient;
            tcpClient.EndConnect(ar);
            ThreadPool.QueueUserWorkItem(SkeletonsRecieving, tcpClient);
        }

        private void SkeletonsRecieving(object state)
        {
            var client = state as TcpClient;
            var networkStream = client.GetStream();
            while (client.Connected)
            {
                var _deserializeJointData = networkStream.DeserializeJointData();
                ThreadPool.QueueUserWorkItem(SkeletsRecieved, _deserializeJointData);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const float _distance = 0.3f;

        private bool mouseClickedRight = false;

        private bool mouseClickedLeft = false;

        private Queue<Point> mousePosStack = new Queue<Point>(10);

        private void SkeletsRecieved(object state)
        {
            var _deserializeJointData = state as TrackedSkelletons;
            if (_deserializeJointData == null)
            {
                return;
            }
            var rightHand = _deserializeJointData.Skelletons.First().Find(jnt=>jnt.JointType == Microsoft.Kinect.JointType.HandRight);
            var leftHand = _deserializeJointData.Skelletons.First().Find(jnt => jnt.JointType == Microsoft.Kinect.JointType.HandLeft);
            var spine = _deserializeJointData.Skelletons.First().Find(jnt => jnt.JointType == Microsoft.Kinect.JointType.Spine);
            
            //label1.Text = rightHand.SkeletPoint.Z.ToString(); //- jetzt wenn due Hand nach vorne bewegt wird dann ein Click machen
            var tmp = rightHand.ScaleOwn(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
            //Cursor.Position = new Point((int) tmp.SkeletPoint.X, (int) tmp.SkeletPoint.Y);
            mousePosStack.Enqueue(new Point((int)tmp.SkeletPoint.X, (int)tmp.SkeletPoint.Y));
            while (mousePosStack.Count > 10)
            {
                mousePosStack.Dequeue();
            }
            var cntX = 0;
            var cntY = 0;
            foreach (var point in mousePosStack)
            {
                cntX += point.X;
                cntY += point.Y;
            }
            Cursor.Position = new Point(cntX / mousePosStack.Count, cntY / mousePosStack.Count);
            var distance = spine.SkeletPoint.Z - leftHand.SkeletPoint.Z;
            label1.Invoke(new MethodInvoker(() =>
                                                {

                                                    try
                                                    {
                                                        label1.Text = distance.ToString();
                                                    }
                                                    catch (Exception)
                                                    {
                                                    }
                                                }));
            if (!mouseClickedLeft && distance > _distance)
            {
                mouseClickedLeft= true;
                mouse_event(MOUSEEVENTF_LEFTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                label1.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        label1.Text += " click";
                    }
                    catch (Exception)
                    {
                    }
                }));
            }
            else if (mouseClickedLeft && distance <= _distance)
            {
                mouseClickedLeft= false;
                mouse_event(MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                label1.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        label1.Text += " unclick";
                    }
                    catch (Exception)
                    {
                    }
                }));
            }else if (!mouseClickedRight && distance < -_distance)
            {
                mouseClickedRight = true;
                mouse_event(MOUSEEVENTF_RIGHTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                label1.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        label1.Text += " clickR";
                    }
                    catch (Exception)
                    {
                    }
                }));
            }
            else if (mouseClickedRight && distance >= -_distance)
            {
                mouseClickedRight = false;
                mouse_event(MOUSEEVENTF_RIGHTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                label1.Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        label1.Text += " unclickR";
                    }
                    catch (Exception)
                    {
                    }
                }));
            }
        }
    }
}
