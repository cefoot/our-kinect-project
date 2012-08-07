using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KinectAddons;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace KinectDepth
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private const double fieldOfView = 57d;//grad aus spec
        private const double pixDist = 40d;
        private const double viewWidth = 80d;
        private double gamma = 0d;
        private double AvrA;
        private double AvrB;
        private double AvrC;
        private double Beta;
        private double Alpha;

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
            Sensor = KinectSensor.KinectSensors[0];
            Sensor.Start();
            Sensor.DepthStream.Enable(DepthImageFormat.Resolution80x60Fps30);

            base.Initialize();
        }

        protected KinectSensor Sensor { get; set; }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            gamma = fieldOfView / viewWidth * pixDist;
            gamma *= Math.PI / 180d;
            DebugFont = Content.Load<SpriteFont>("DebugFont");
            StartVector = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height);
            RightVector = new Vector2((float)Math.Cos((Math.PI - gamma) / 2), -(float)Math.Sin((Math.PI - gamma) / 2));
            // TODO: use this.Content to load your game content here
        }

        protected Vector2 LeftVector { get; set; }

        protected Vector2 RightVector { get; set; }

        protected Vector2 StartVector { get; set; }

        protected SpriteFont DebugFont { get; set; }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        protected override void Dispose(bool disposing)
        {
            Sensor.DepthStream.Disable();
            Sensor.Stop();
            base.Dispose(disposing);
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

            var depthImageFrame = Sensor.DepthStream.OpenNextFrame(500);
            if (depthImageFrame == null)
            {
                base.Update(gameTime);
                return;
            }
            var pixlData = new short[depthImageFrame.PixelDataLength];
            depthImageFrame.CopyPixelDataTo(pixlData);
            depthImageFrame.Dispose();

            double left = 0d;
            double right = 0d;
            double i = 0d;
            for (int y = 25; y < 35; y++)
            {
                for (int x = 15; x < 25; x++)
                {
                    var lineCoord = y * 80 + x;
                    left += ((ushort)pixlData[lineCoord]) >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    lineCoord = y * 80 + x + (int)pixDist;
                    right += ((ushort)pixlData[lineCoord]) >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    i++;
                }
            }
            AvrA = left / i;
            AvrB = right / i;

            //AvrC = Math.Sqrt(AvrA * AvrA + AvrB * AvrB + 2d * AvrB * AvrA * Math.Cos(gamma));
            AvrC = (new Vector3((float)AvrA, 0, 0) - new Vector3((float)(Math.Cos(gamma) * AvrB), (float)(Math.Sin(gamma) * AvrB), 0)).Length();
            Beta = Math.Acos((AvrA * AvrA + AvrC * AvrC - AvrB * AvrB) / (2d * AvrA * AvrC));

            Alpha = Math.Acos((AvrB * AvrB + AvrC * AvrC - AvrA * AvrA) / (2d * AvrB * AvrC));

            var vec = RightVector;
            vec.Normalize();
            RightVector = vec;

            RightVector.Normalize();
            LeftVector = new Vector2(-RightVector.X, RightVector.Y);
            if (AvrA > 1 && AvrB > 1)
            {
                LeftVector = LeftVector * (float)AvrB * 0.2f;
                RightVector = RightVector * (float)AvrA * 0.2f;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            float y = 1;
            //spriteBatch.DrawString(DebugFont, String.Format("AvrA:{0}mm", Math.Round(AvrA, 2)), new Vector2(0, y++ * 20), Color.White);
            //spriteBatch.DrawString(DebugFont, String.Format("AvrB:{0}mm", Math.Round(AvrB, 2)), new Vector2(0, y++ * 20), Color.White);
            //spriteBatch.DrawString(DebugFont, String.Format("AvrC:{0}mm", Math.Round(AvrC, 2)), new Vector2(0, y++ * 20), Color.White);
            //spriteBatch.DrawString(DebugFont, String.Format("Beta:{0} grad", Math.Round(Beta * 180 / Math.PI, 2)), new Vector2(0, y++ * 20), Color.White);
            //spriteBatch.DrawString(DebugFont, String.Format("Alpha:{0} grad", Math.Round(Alpha * 180 / Math.PI, 2)), new Vector2(0, y++ * 20), Color.White);
            //spriteBatch.DrawString(DebugFont, String.Format("Start:X{0}Y{1}", Math.Cos((Math.PI - gamma) / 2), Math.Sin((Math.PI - gamma) / 2)), new Vector2(0, y++ * 20), Color.White);
            //spriteBatch.DrawString(DebugFont, String.Format("Vec1:{0}", LeftVector), new Vector2(0, y++ * 20), Color.White);
            //spriteBatch.DrawString(DebugFont, String.Format("Vec2:{0}", RightVector), new Vector2(0, y++ * 20), Color.White);

            spriteBatch.DrawString(DebugFont, Math.Round(Beta * 180 / Math.PI, 2).ToString(), StartVector + RightVector, Color.White);
            spriteBatch.DrawString(DebugFont, Math.Round(Alpha * 180 / Math.PI, 2).ToString(), StartVector + LeftVector, Color.White);

            var line = new PrimitiveLine(GraphicsDevice);
            line.Colour = Color.Red;
            line.AddVector(StartVector);
            line.AddVector(StartVector + RightVector);
            line.AddVector(StartVector + LeftVector);
            line.AddVector(StartVector);
            line.Render(spriteBatch);
            line.ClearVectors();
            line.Position = StartVector + RightVector;
            line.CreateCircle(10, 20);
            line.Render(spriteBatch);
            line.ClearVectors();
            line.Position = StartVector + LeftVector;
            line.CreateCircle(10, 20);
            line.Render(spriteBatch);

            spriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
