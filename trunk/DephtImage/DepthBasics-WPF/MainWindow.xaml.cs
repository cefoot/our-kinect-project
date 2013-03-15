//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int imageWidth;
        private int imageHeight;
        private int currentAverageCycles=0;
        private int currentIgnoreCycles=0;
        private const int minDistance=80;
        private const int maxDistance = 1000;
        private const int preMinX = 000;
        private const int preMaxX = 1000;
        
        private const int preMinY = 000;
        private const int preMaxY = 1000;

        private const int maxAverageCycles=200;
        private const int maxIgnoreFrames = 10;
        private Boolean stopIgnoring = false;
        private float kinectDepthFOW = 45.6f;
        private Boolean imageSizedSet = false;
        private Boolean backgroundIsCaptured=false;
        private Configuration config;
        private static float minDifference=50;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private short[] depthPixels;
        private DepthImagePixel[] depthImagePixel;

        private CoordinateMapper coordMaper;

        /// <summary>
        /// Intermediate storage for the depth data converted to color
        /// </summary>
        private byte[] colorPixels;
        private static int maxPointsToTrack = 400;
        

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                
                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new short[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the depth pixels we'll receive
                this.depthImagePixel = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.colorPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;

                this.coordMaper = this.sensor.CoordinateMapper;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        public void setImageDimension(long length)
        {

            if (length == 80 * 60)
            {
                this.imageWidth = 80;
                this.imageHeight = 60;
            }
            else if (length == 320 * 240)
            {
                this.imageWidth = 320;
                this.imageHeight = 240;
            }
            else if (length == 640 * 480)
            {
                this.imageWidth = 640;
                this.imageHeight = 480;
            }
            else
            {
                //unknown resolution, exit!
                throw new Exception("i dont know how what dimension the inputimage is!!Help!!");
            }
            this.imageSizedSet = true;
        }
       
        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {

                   
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyPixelDataTo(this.depthPixels);

                    depthFrame.CopyDepthImagePixelDataTo(depthImagePixel);

                     //first set image size once
                    if (!this.imageSizedSet)
                    {
                        this.setImageDimension(this.depthPixels.Length);
                    }
                    configureBackground();
                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    
                    //for (int x = MainWindow.preMinX; x < MainWindow.preMaxX; ++x)
                    //{
                        //for (int y = MainWindow.preMinY; y < MainWindow.preMaxY; ++y)
                        //{
                            //Hier wieder alte for-Schleife einbauen, da ansonsten das colorPixelIndex++ nicht mehr funktioniert...
                    int currentPoint = 0;
                    
                    for(int i=this.depthPixels.Length-1 ;i>0;--i)
                    {
                        
                            //i = x + y * this.imageHeight;

                            // discard the portion of the depth that contains only the player index
                            int depth = (ushort)(this.depthPixels[i] >> DepthImageFrame.PlayerIndexBitmaskWidth);
                            //((ushort)pixel) >> DepthImageFrame.PlayerIndexBitmaskWidth;
                                                   
                            
                            //System.Diagnostics.Debug.WriteLine("intensity:" + intensity + " depth:" + depth);
                            if (currentPoint<maxPointsToTrack  && depth > MainWindow.minDistance && depth < MainWindow.maxDistance && MainWindow.preMinX < oneDtoTwoD(i).Item1 && MainWindow.preMaxX > oneDtoTwoD(i).Item1 && Math.Abs(config.getBackgroundAt(i) - depth) > MainWindow.minDifference)
                            {
                                currentPoint++;

                                DepthImagePoint newDepthPoint = new DepthImagePoint();
                                newDepthPoint.X = oneDtoTwoD(i).Item1;
                                newDepthPoint.Y = oneDtoTwoD(i).Item2;
                                newDepthPoint.Depth = depth;

                                SkeletonPoint tempPoint = coordMaper.MapDepthPointToSkeletonPoint(depthFrame.Format, newDepthPoint);
                                float heightAboveGround = this.config.ComputeDistanceToBoard(new Vector3(tempPoint.X, tempPoint.Y, tempPoint.Z));
                                //Height and Position of current Point
                                label1.Content = heightAboveGround.ToString();
                                label2.Content = tempPoint.X.ToString();
                                label3.Content = tempPoint.Y.ToString();
                                label4.Content = tempPoint.Z.ToString();
                                label8.Content = depth.ToString();


                                Vector3 backgroundPoint = config.getBackground3DAt(i);

                                //newDepthPoint.Depth = (int)Math.Round(config.getBackgroundAt(i));
                                //SkeletonPoint backgroundPoint = coordMaper.MapDepthPointToSkeletonPoint(depthFrame.Format, newDepthPoint);

                                label5.Content = backgroundPoint.X.ToString();
                                label6.Content = backgroundPoint.Y.ToString();
                                label7.Content = backgroundPoint.Z.ToString();

                                //System.Diagnostics.Debug.WriteLine(heightAboveGround+" at "+oneDtoTwoD(i)+" "+" "+mapYtoAngle(oneDtoTwoD(i).Item2));
                                if (heightAboveGround < 1)
                                {

                                    // Write out blue byte
                                    this.colorPixels[colorPixelIndex++] = 0;

                                    // Write out green byte
                                    this.colorPixels[colorPixelIndex++] = 255;


                                    // Write out red byte                        
                                    this.colorPixels[colorPixelIndex++] = 0;
                                }
                                else
                                {
                                    // Write out blue byte
                                    this.colorPixels[colorPixelIndex++] = 0;

                                    // Write out green byte
                                    this.colorPixels[colorPixelIndex++] = 0;


                                    // Write out red byte                        
                                    this.colorPixels[colorPixelIndex++] = 155;
                                                               

                                    
                                }

                            }
                            else if (depth == ushort.MaxValue)
                            {

                                // to convert to a byte we're looking at only the lower 8 bits
                                // by discarding the most significant rather than least significant data
                                // we're preserving detail, although the intensity will "wrap"
                                // add 1 so that too far/unknown is mapped to black
                                byte intensity = 0;//(byte)(((ushort)config.getBackgroundAt(i) + 1) & byte.MaxValue);

                                // Write out blue byte
                                this.colorPixels[colorPixelIndex++] = intensity;

                                // Write out green byte
                                this.colorPixels[colorPixelIndex++] = intensity;


                                // Write out red byte                        
                                this.colorPixels[colorPixelIndex++] = intensity;
                            }
                            else
                            {
                                // to convert to a byte we're looking at only the lower 8 bits
                                // by discarding the most significant rather than least significant data
                                // we're preserving detail, although the intensity will "wrap"
                                // add 1 so that too far/unknown is mapped to black
                                byte intensity = 0;// (byte)((depth + 1) & byte.MaxValue);

                                // Write out blue byte
                                this.colorPixels[colorPixelIndex++] = intensity;

                                // Write out green byte
                                this.colorPixels[colorPixelIndex++] = intensity;


                                // Write out red byte                        
                                this.colorPixels[colorPixelIndex++] = intensity;
                            }
                            // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                            // If we were outputting BGRA, we would write alpha here.
                            ++colorPixelIndex;
                        }
                    

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        private int findHeightAboveGround(int depth, int i)
        {
            int xPos = oneDtoTwoD(i).Item1;
            int yPos = oneDtoTwoD(i).Item2;
            double minDist = 15;
            int minPos = int.MaxValue;
            
            for (int yCount = yPos + 1; yCount < this.imageHeight; yCount++)
            {
                float backDepth = config.getBackgroundAt(xPos + yPos * this.imageHeight);
                //double dist=calcDist(xPos, yPos, depth, xPos, yCount, backDepth);

                //if (dist < minDist)
                //{
                //    minDist = dist;
                //    minPos = i;
                //}
                if (Math.Abs(depth - backDepth) < minDist - Math.Abs(yPos - yCount))
                {
                    return yCount;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("depth: "+depth+" backdepth: "+backDepth+" yPos "+yPos+" yCount "+yCount);
                }
            }

            return int.MaxValue;

        }

        private double calcDist(int xPos, int yPos, int depth, int xPos_2, int yCount, float backDepth)
        {
            return Math.Sqrt(Math.Pow((yPos - yCount),2) + Math.Pow(((float)depth - backDepth),2));
        }

        private int twoDtoOneD(int x, int y)
        {
            return x + this.imageHeight * y;
        }

        private Tuple<int, int> oneDtoTwoD(int oneDpos)
        {
            int y = oneDpos / this.imageWidth;
            int x = oneDpos - y * this.imageWidth;
            Tuple<int, int> twoDPos = new Tuple<int, int>(x, y);
            return twoDPos;

        }

        private float mapYtoAngle(int y)
        {
            return (this.kinectDepthFOW / this.imageHeight) * y;
        }

        private void configureBackground()
        {
            //configure everything
            if (this.config == null && this.depthPixels !=null)
            {
                this.config = new Configuration(this.imageWidth, this.imageHeight, this.depthPixels);
                
            }
            if (!this.backgroundIsCaptured && this.depthPixels != null && this.config != null)
            {
                
                if (this.stopIgnoring)
                {
                    Console.Write(".");
                    this.config.addToBackground(this.depthPixels);
                    this.currentAverageCycles++;
                    if (this.currentAverageCycles >= MainWindow.maxAverageCycles)
                    {
                        Console.WriteLine("Done.");
                        //create 3d background
                        this.config.create3DBackgorund(this.coordMaper, DepthImageFormat.Resolution640x480Fps30);
                        this.config.calculatePlane();
                        this.backgroundIsCaptured = true;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    this.currentIgnoreCycles++;
                    if (currentIgnoreCycles >= MainWindow.maxIgnoreFrames)
                    {
                        this.stopIgnoring = true;
                        Console.WriteLine("Starting to record background");
                    }
                }
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.statusBarText.Text = string.Format("{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                this.statusBarText.Text = string.Format("{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
            }
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                // will not function on non-Kinect for Windows devices
                try
                {
                    if (this.checkBoxNearMode.IsChecked.GetValueOrDefault())
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }
}