using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        private void SkeletsRecieved(object state)
        {
            var _deserializeJointData = state as TrackedSkelletons;
            if (_deserializeJointData == null)
            {
                return;
            }
            var rightHand = _deserializeJointData.Skelletons.First().Find(jnt=>jnt.JointType == Microsoft.Kinect.JointType.HandRight);
            //label1.Text = rightHand.SkeletPoint.Z.ToString(); //- jetzt wenn due Hand nach vorne bewegt wird dann ein Click machen
            var tmp = rightHand.ScaleOwn(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
            Cursor.Position = new Point((int) tmp.SkeletPoint.X, (int) tmp.SkeletPoint.Y);
        }
    }
}
