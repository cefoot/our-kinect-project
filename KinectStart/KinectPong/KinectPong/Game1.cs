using System;
using System.Collections.Generic;
using System.Linq;
using Coding4Fun.Kinect.Wpf;
using Kinect;
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
        Rectangle paddleLeft;
        Rectangle paddleRight;
        Color Colori;
        private Texture2D _ballTexture;
        private Ball _ball;
        private SoundEffect _boomSound;
        private SpriteFont _spriteFont;
        private float TmpPos;

        protected Vector2 BallStartPosition { get; set; }


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
            var plyr1Hnd = Optimize(player1.Joints[JointType.HandRight]).ScaleOwn(
                GraphicsDevice.Viewport.TitleSafeArea.Width,
                GraphicsDevice.Viewport.TitleSafeArea.Height);
            var plyr2Head = player2.Joints[JointType.Head];
            var plyr2Hnd = Optimize(player2.Joints[JointType.HandRight]).ScaleOwn(
                GraphicsDevice.Viewport.TitleSafeArea.Width,
                GraphicsDevice.Viewport.TitleSafeArea.Height);


            if(plyr1Head.Position.X < plyr2Head.Position.X)
            {
                TmpPos = plyr1Hnd.Position.Y;
                LeftPlayerY = (int) plyr1Hnd.Position.Y - GetMiddleDeltaFromPlayer(player1);
                RightPlayerY = (int)plyr2Hnd.Position.Y - GetMiddleDeltaFromPlayer(player2);
            }
            else
            {

                TmpPos = plyr2Hnd.Position.Y;
                RightPlayerY = (int)plyr1Hnd.Position.Y - GetMiddleDeltaFromPlayer(player1);
                LeftPlayerY = (int)plyr2Hnd.Position.Y - GetMiddleDeltaFromPlayer(player2);
            }
            
        }

        private int GetMiddleDeltaFromPlayer(Skeleton plyr)
        {
            var scaledHipCenter = plyr.Joints[JointType.HipCenter].ScaleTo(
                GraphicsDevice.Viewport.TitleSafeArea.Width,
                GraphicsDevice.Viewport.TitleSafeArea.Height);

            var scaledShoulderCenter = plyr.Joints[JointType.ShoulderCenter].ScaleTo(
                GraphicsDevice.Viewport.TitleSafeArea.Width,
                GraphicsDevice.Viewport.TitleSafeArea.Height);


            var middlePosition = (scaledShoulderCenter.Position.Y+scaledHipCenter.Position.Y)/2f;
            return (int)middlePosition - GraphicsDevice.Viewport.TitleSafeArea.Height/2;
        }

        private static Joint Optimize(Joint jnt)
        {
            jnt.Position = new SkeletonPoint { X = 0f,
            Y = jnt.Position.Y * 2f,
            Z = 0f};
            return jnt;
        }

        protected int RightPlayerY { get; set; }

        protected int LeftPlayerY { get; set; }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            paddleLeft = new Rectangle(5, 10, 10, 100);
            paddleRight = new Rectangle(
                GraphicsDevice.Viewport.TitleSafeArea.Width - paddleLeft.Width - paddleLeft.Left, 
                10,
                paddleLeft.Width,
                paddleLeft.Height);
            dummyTexture = new Texture2D(GraphicsDevice, 50, 100);
            var data = new Color[50 * 100];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            dummyTexture.SetData(data);

            Colori = Color.Chocolate;
            //dummyTexture.SetData(new Color[] { Color.Red });
            _ballTexture = Content.Load<Texture2D>("ball");
            

            BallStartPosition = new Vector2(
                GraphicsDevice.Viewport.TitleSafeArea.Width/2f, GraphicsDevice.Viewport.TitleSafeArea.Height/2f);
            _ball = new Ball(BallStartPosition, CreateNewSpeed());

            _boomSound = Content.Load<SoundEffect>("boom");
            _spriteFont = Content.Load<SpriteFont>("Arial");


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            KinectSensor.KinectSensors[0].Stop();
        }

        private Vector2 LeftPaddleSpeed { get; set; }
        private Vector2 RightPaddleSpeed { get; set; }

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

            LeftPaddleSpeed = new Vector2
                                  {
                                      X = 0,
                                      Y = (float) ((LeftPlayerY - paddleLeft.Y)/gameTime.ElapsedGameTime.TotalSeconds)
                                  };
            RightPaddleSpeed = new Vector2
            {
                X = 0,
                Y = (float)((RightPlayerY - paddleRight.Y) / gameTime.ElapsedGameTime.TotalSeconds)
            };
            paddleLeft.Y = LeftPlayerY;
            paddleRight.Y = RightPlayerY;


            var nextStep = _ball.GetNextStep(gameTime);
            var rectangle = _ball.CreateFrame(nextStep);
            if (rectangle.Top < 0 || rectangle.Bottom > GraphicsDevice.Viewport.TitleSafeArea.Height)
            {
                _ball.Speed.Y *= -1;
            }
            if (rectangle.Intersects(paddleRight))
            {
                _ball.Speed.X *= -1.1F;
                _ball.Speed += 0.5f*RightPaddleSpeed;
            }
            else if (rectangle.Intersects(paddleLeft))
            {
                _ball.Speed.X *= -1.1F;
                _ball.Speed += 0.5f*LeftPaddleSpeed;
            }
            else if (rectangle.Left < paddleLeft.Right)
            {
                GameOver();
                RightScore++;
            }
            else if(rectangle.Right > paddleRight.Left)
            {
                GameOver();
                LeftScore++;
            }
            _ball.Step(gameTime);

            base.Update(gameTime);
        }

        protected int LeftScore { get; set; }

        protected int RightScore { get; set; }

        private void GameOver()
        {
            _boomSound.Play();
            _ball.Speed = CreateNewSpeed();
            _ball.Location = BallStartPosition;
        }

        Random rand = new Random();
        private Vector2 CreateNewSpeed()
        {
            return new Vector2(GetRandVal(), GetRandVal());
        }

        private float GetRandVal()
        {
            var d = (float) ((rand.NextDouble() - .5)*200f);
            if(d<0)
            {
                return d - 200f;
            }
            return d + 200f;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(dummyTexture, paddleLeft, Colori);
            spriteBatch.Draw(dummyTexture, paddleRight, Colori);
            spriteBatch.Draw(_ballTexture, _ball.Location, null, Color.White, 0f, new Vector2(_ball.Frame.Width/2, _ball.Frame.Height/2), 0.2f, SpriteEffects.None, 0);
            var scoreString = String.Format("{0}:{1}",LeftScore,RightScore);
            var measureString = _spriteFont.MeasureString(scoreString);
            measureString.Y = 10;
            measureString.X = GraphicsDevice.Viewport.TitleSafeArea.Width / 2 - measureString.X / 2;

            spriteBatch.DrawString(_spriteFont, scoreString, measureString, Color.White);
            DrawDebugStuff(spriteBatch);
            spriteBatch.End();


            //base.Draw(gameTime);
        }

        private void DrawDebugStuff(SpriteBatch spriteBatch)
        {
            //its clean code :-)
            return;
            int i = 1;
            var msg = String.Format("Ball Position:{0}", _ball.Location);
            var measureString = _spriteFont.MeasureString(msg);
            measureString.Y = GraphicsDevice.Viewport.TitleSafeArea.Height - measureString.Y * i++;
            measureString.X = GraphicsDevice.Viewport.TitleSafeArea.Width / 2 - measureString.X / 2;
            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.White);

            msg = String.Format("Ball Speed:{0}", _ball.Speed);
            measureString = _spriteFont.MeasureString(msg);
            measureString.Y = GraphicsDevice.Viewport.TitleSafeArea.Height - measureString.Y * i++;
            measureString.X = GraphicsDevice.Viewport.TitleSafeArea.Width / 2 - measureString.X / 2;
            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.White);

            msg = String.Format("PaddleSpeed:Left{0} Right{1}", LeftPaddleSpeed, RightPaddleSpeed);
            measureString = _spriteFont.MeasureString(msg);
            measureString.Y = GraphicsDevice.Viewport.TitleSafeArea.Height - measureString.Y * i++;
            measureString.X = GraphicsDevice.Viewport.TitleSafeArea.Width / 2 - measureString.X / 2;
            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.White);

            msg = String.Format("LeftHandPosition:Pos{0}", TmpPos);
            measureString = _spriteFont.MeasureString(msg);
            measureString.Y = GraphicsDevice.Viewport.TitleSafeArea.Height - measureString.Y * i++;
            measureString.X = GraphicsDevice.Viewport.TitleSafeArea.Width / 2 - measureString.X / 2;
            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.White);
        }

        public class Ball
        {
            public Ball(Vector2 startPosition, Vector2 startSpeed)
            {
                Location = startPosition;
                Speed = startSpeed;
            }

            public Rectangle Frame
            {
                get {
                    return CreateFrame(Location);
                }
            }

            public Rectangle CreateFrame(Vector2 location)
            {
                var size = 43;
                var frame = new Rectangle(
                    x:(int)location.X,// - size/2,
                    y:(int)location.Y,// - size/2,
                    width:size,
                    height:size
                    
                    );
                return frame;
            }

            public Vector2 Location { get; set; }
            public Vector2 Speed;

            public Vector2 GetNextStep(GameTime gameTime)
            {
                return Location + Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            public void Step(GameTime gameTime)
            {
                Location = GetNextStep(gameTime);
            }
        }
    }
}