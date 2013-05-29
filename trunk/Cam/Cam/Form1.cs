using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace Cam
{
    public partial class Form1 : Form
    {
        private KinectSensor _kinectSensor;
        private byte[] pixels;

        public Form1()
        {
            InitializeComponent();
            _kinectSensor = KinectSensor.KinectSensors[0];
            pixels = new byte[_kinectSensor.ColorStream.FramePixelDataLength];

            wBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight,
                96, 96, PixelFormats.Bgra32, null);
            _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            _kinectSensor.ColorFrameReady += _kinectSensor_ColorFrameReady;
        }

        void _kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
