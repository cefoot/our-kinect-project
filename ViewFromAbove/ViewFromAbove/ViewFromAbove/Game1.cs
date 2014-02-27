using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;
using Microsoft.Kinect;
using Color = Microsoft.Xna.Framework.Color;


namespace ViewFromAbove
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        KinectSensor kinect;
        DepthImageFrame depthFrame;
        GraphicsDeviceManager graphics;
        private KinectSensor sensor;
        SpriteBatch spriteBatch;
        public Array colorPixels { get; set; }
        static int imageWidth = 640;
        static int imageHeight = 480;
        //public int[,] blackWhiteImage = new int[imageWidth,imageHeight];
        Rectangle tracedSize;
        UInt32[] blackWhiteImage;
        UInt32[] circle;
        List<List<Point>> clusters;

        List<Point> points = new List<Point>();
        private Texture2D clusterTexture;
        private Texture2D backgroundText;

        private void initKinectSensor()
        {
            this.kinect = KinectSensor.KinectSensors.Where(x => x.Status == KinectStatus.Connected).FirstOrDefault();
            this.kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(Sensor_DepthFrameReady);
            this.kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            this.kinect.Start();
        }

        void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (depthFrame = e.OpenDepthImageFrame())
            {
                if (this.depthFrame != null)
                {
                    DepthImagePixel[] array = depthFrame.GetRawPixelData();
                    for (int i = 0; i < array.Length; i++)
                    {
                        blackWhiteImage[i] = (uint)array[i].Depth;
                    }

                }
                filter(1000, 2000);
            }

        }

        //init testimage

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public void InitKinect()
        {
            sensor = KinectSensor.KinectSensors.First(kinect => kinect.Status == KinectStatus.Connected);
        }

        public void initRandomImage(int numberOfPoints, int pointRadius)
        {
            //create background noise
            Random r2 = new Random();
            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    blackWhiteImage[i + j * imageWidth] = (uint)r2.Next(6) * (uint)r2.Next(6) * (uint)r2.Next(6);
                }
            }

            //create some points and create higher density of points around them
            Random r1 = new Random();
            for (int k = 0; k < numberOfPoints; k++)
            {
                Vector2 pointOrigin = new Vector2(r1.Next(pointRadius, imageWidth - pointRadius), r1.Next(pointRadius, imageHeight - pointRadius));

                Random r = new Random();
                for (int i = -pointRadius; i < pointRadius; i++)
                {
                    for (int j = -pointRadius; j < pointRadius; j++)
                    {
                        blackWhiteImage[(int)pointOrigin.X + i + ((int)pointOrigin.Y + j) * imageWidth] = (uint)Math.Pow(r.Next(15), 2);
                    }
                }
            }

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            tracedSize = GraphicsDevice.PresentationParameters.Bounds;
            blackWhiteImage = new UInt32[imageWidth * imageHeight];
            circle = new UInt32[10 * 10];
            initCircle();
            this.initRandomImage(10, 10);

            this.initKinectSensor();


            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            clusterTexture = new Texture2D(GraphicsDevice, 1, 1);
            clusterTexture.SetData(new[] { Color.White });
            backgroundText = new Texture2D(GraphicsDevice, imageWidth, imageHeight);
            //backgroundText.SetData(new[] { Color.White });
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            clusters = DBSCAN.GetClusters(points, 10, 8);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            GraphicsDevice.Textures[0] = null;

            backgroundText.SetData<UInt32>(blackWhiteImage, 0, imageWidth * imageHeight);

            spriteBatch.Draw(backgroundText, new Rectangle(0, 0, imageWidth, imageHeight), Color.White);
            int count = 0;

            Debug.WriteLine("elapsedtime:" + gameTime.ElapsedGameTime);
            foreach (List<Point> cluster in clusters)
            {
                count++;
                Point center = getCenter(cluster);
                spriteBatch.Draw(clusterTexture, new Rectangle(center.X - 5, center.Y - 5, 12, 12), Color.Green);

            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        void initCircle()
        {
            for (int i = 0; i < circle.Length; i++)
            {
                circle[i] = (uint)200;
            }
        }


        public void filter(uint thresholdLow, uint thresholdHigh)
        {
            var tmpPoints = new List<Point>();
            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    if (blackWhiteImage[i + j * imageWidth] > thresholdLow && blackWhiteImage[i + j * imageWidth] < thresholdHigh)
                    {
                        tmpPoints.Add(new Point(i, j));
                    }

                }
            }
            points = tmpPoints;
        }

        public Point getCenter(List<Point> cluster)
        {
            Point center = new Point(0, 0);
            foreach (Point point in cluster)
            {
                center.X += point.X;
                center.Y += point.Y;
            }

            center.X /= cluster.Count;
            center.Y /= cluster.Count;

            return center;


        }







    }
}
