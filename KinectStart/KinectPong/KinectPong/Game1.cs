using System;
using System.Collections.Generic;
using System.Linq;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Coding4Fun.Kinect.Common;

namespace KinectPong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D dummyTexture;
        Rectangle rectangleLeft;
        Rectangle rectangleRight;
        Color Colori;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            var kinectSensor = KinectSensor.KinectSensors[0];
            kinectSensor.SkeletonStream.Enable();
            kinectSensor.SkeletonFrameReady += KinectSensorSkeletonFrameReady;
            kinectSensor.Start();
            base.Initialize();
        }

        private void KinectSensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var openSkeletonFrame = e.OpenSkeletonFrame();
            if(openSkeletonFrame == null) return;
            var skeletsData = new Skeleton[openSkeletonFrame.SkeletonArrayLength];
            openSkeletonFrame.CopySkeletonDataTo(skeletsData);

            //TODO linq nur 2 holen
            var trackedSkelets = skeletsData.ToList().Where(skelet => skelet.TrackingState == SkeletonTrackingState.Tracked);
            var enumerator = trackedSkelets.GetEnumerator();
            if(!enumerator.MoveNext())
            {
                return;
            }
            var player1 = enumerator.Current;
            if (!enumerator.MoveNext())
            {
                return;
            }
            var player2 = enumerator.Current;
            UpdateCoords(player1, player2);
        }

        private void UpdateCoords(Skeleton player1, Skeleton player2)
        {
            var plyr1Head = player1.Joints[JointType.Head];
            var plyr1Hnd = player1.Joints[JointType.HandRight].ScaleTo(
            GraphicsDevice.Viewport.TitleSafeArea.Width ,
            GraphicsDevice.Viewport.TitleSafeArea.Height);
            var plyr2Head = player2.Joints[JointType.Head];
            var plyr2Hnd = player2.Joints[JointType.HandRight].ScaleTo(
            GraphicsDevice.Viewport.TitleSafeArea.Width,
            GraphicsDevice.Viewport.TitleSafeArea.Height);

            if(plyr1Head.Position.X < plyr2Head.Position.X)
            {
                LeftPlayerY = (int) plyr1Hnd.Position.Y;
                RightPlayerY = (int) plyr2Hnd.Position.Y;
            }
            else
            {
                RightPlayerY = (int) plyr1Hnd.Position.Y;
                LeftPlayerY = (int) plyr2Hnd.Position.Y;
            }
            
        }

        protected int RightPlayerY
        {
            get { return rectangleRight.Y; }
            set { rectangleRight.Y = value; }
        }

        protected int LeftPlayerY
        {
            get { return rectangleLeft.Y; }
            set { rectangleLeft.Y = value; }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            rectangleLeft = new Rectangle(5, 10, 50, 100);
            rectangleRight = new Rectangle(
                GraphicsDevice.Viewport.TitleSafeArea.Width - rectangleLeft.Width - rectangleLeft.Left, 
                10,
                rectangleLeft.Width,
                rectangleLeft.Height);
            dummyTexture = new Texture2D(GraphicsDevice, 50, 100);
            var data = new Color[50 * 100];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            dummyTexture.SetData(data);

            Colori = Color.Chocolate;
            //dummyTexture.SetData(new Color[] { Color.Red });



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

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(dummyTexture, rectangleLeft, Colori);
            spriteBatch.Draw(dummyTexture, rectangleRight, Colori);
            spriteBatch.End();


            //base.Draw(gameTime);
        }
    }
}
