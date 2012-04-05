using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using KinectAddons;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Kinect;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using MessageBoxIcon = System.Windows.Forms.MessageBoxIcon;

namespace StickmansSpeech
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private IEnumerable<Skeleton> _trackedSkeletons;
        private KinectSensor _kinectSensor;
        private double _anglesource;
        private SpriteFont _spriteFont;

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
            _kinectSensor = KinectSensor.KinectSensors[0];
            _kinectSensor.SkeletonStream.Enable();
            _kinectSensor.SkeletonFrameReady += KinectSensorSkeletonFrameReady;
            _kinectSensor.Start();
            base.Initialize();
        }

        private void KinectSensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var openSkeletonFrame = e.OpenSkeletonFrame();
            if (openSkeletonFrame == null) return;
            var skeletsData = new Skeleton[openSkeletonFrame.SkeletonArrayLength];
            openSkeletonFrame.CopySkeletonDataTo(skeletsData);
            _trackedSkeletons = skeletsData.ToList().Where(skelet => skelet.TrackingState == SkeletonTrackingState.Tracked);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("Arial");

            _kinectSensor.AudioSource.Start();

            _kinectSensor.AudioSource.SoundSourceAngleChanged += AudioSource_SoundSourceAngleChanged;

            // TODO: use this.Content to load your game content here
        }

        void AudioSource_SoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            if(e.ConfidenceLevel > .3)
            {
                _anglesource = e.Angle;
            }
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
            
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            //_trackedSkeletons.AsParallel().ForAll(skelet => Drawstickman(skelet, spriteBatch));
            foreach (var trackedSkeleton in _trackedSkeletons)
            {

                Drawstickman(trackedSkeleton, spriteBatch);
            }
            DrawDebugStuff(spriteBatch);
            // TODO: Add your drawing code here
            spriteBatch.End();
            //base.Draw(gameTime);
        }

        private void DrawDebugStuff(SpriteBatch spriteBatch)
        {
            int i = 1;
            var msg = String.Format("Ball Position:{0}", _anglesource);
            var measureString = _spriteFont.MeasureString(msg);
            measureString.Y = GraphicsDevice.Viewport.TitleSafeArea.Height - measureString.Y*i++;
            measureString.X = GraphicsDevice.Viewport.TitleSafeArea.Width/2 - measureString.X/2;
            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.Black);
        }


        private SpeechRecognitionEngine CreateSpeechRecognizer()
        {
            RecognizerInfo ri = SpeechUtils.GetKinectRecognizer();
            if (ri == null)
            {
                MessageBox.Show(
                    @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed.",
                    "Failed to load Speech SDK",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return null;
            }

            SpeechRecognitionEngine sre;
            try
            {
                sre = new SpeechRecognitionEngine(ri.Id);
            }
            catch
            {
                MessageBox.Show(
                    @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed and configured.",
                    "Failed to load Speech SDK",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return null;
            }

            var colors = new Choices();
            colors.Add("red");
            colors.Add("green");
            colors.Add("blue");

            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(colors);

            // Create the actual Grammar instance, and then load it into the speech recognizer.
            var g = new Grammar(gb);

            sre.LoadGrammar(g);
            sre.SpeechRecognized += this.SreSpeechRecognized;
            sre.SpeechHypothesized += this.SreSpeechHypothesized;
            sre.SpeechRecognitionRejected += this.SreSpeechRecognitionRejected;

            return sre;
        }

        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Drawstickman(Skeleton trackedSkeleton, SpriteBatch spriteBatch)
        {
            var shoulderCenter = trackedSkeleton.Joints[JointType.ShoulderCenter].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var hipCenter = trackedSkeleton.Joints[JointType.HipCenter].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var footLeft = trackedSkeleton.Joints[JointType.FootLeft].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var footRight = trackedSkeleton.Joints[JointType.FootRight].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var handLeft = trackedSkeleton.Joints[JointType.HandLeft].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var handRight = trackedSkeleton.Joints[JointType.HandRight].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var elbowLeft = trackedSkeleton.Joints[JointType.ElbowLeft].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var elbowRight = trackedSkeleton.Joints[JointType.ElbowRight].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var kneeLeft = trackedSkeleton.Joints[JointType.KneeLeft].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var kneeRight = trackedSkeleton.Joints[JointType.KneeRight].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var head = trackedSkeleton.Joints[JointType.Head].ScaleOwn(GraphicsDevice.Viewport.TitleSafeArea.Width /2, GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            var primitiveLine = new PrimitiveLine(GraphicsDevice);
            primitiveLine.Depth = 0;
            primitiveLine.AddVector(shoulderCenter.Position.Convert());
            primitiveLine.AddVector(hipCenter.Position.Convert());
            primitiveLine.Colour = Color.Black;
            primitiveLine.Render(spriteBatch);

            primitiveLine.ClearVectors();
            primitiveLine.AddVector(shoulderCenter.Position.Convert());
            primitiveLine.AddVector(elbowRight.Position.Convert());
            primitiveLine.AddVector(handRight.Position.Convert());
            primitiveLine.Render(spriteBatch);
            primitiveLine.ClearVectors();
            primitiveLine.AddVector(shoulderCenter.Position.Convert());
            primitiveLine.AddVector(elbowLeft.Position.Convert());
            primitiveLine.AddVector(handLeft.Position.Convert());
            primitiveLine.Render(spriteBatch);
            primitiveLine.ClearVectors();
            primitiveLine.AddVector(hipCenter.Position.Convert());
            primitiveLine.AddVector(kneeRight.Position.Convert());
            primitiveLine.AddVector(footRight.Position.Convert());
            primitiveLine.Render(spriteBatch);
            primitiveLine.ClearVectors();
            primitiveLine.AddVector(hipCenter.Position.Convert());
            primitiveLine.AddVector(kneeLeft.Position.Convert());
            primitiveLine.AddVector(footLeft.Position.Convert());
            primitiveLine.Render(spriteBatch);

            primitiveLine.ClearVectors();
            primitiveLine.Position = head.Position.Convert();
            //primitiveLine.CreateCircle((head.Position.Convert() - shoulderCenter.Position.Convert()).Length(), 20);
            primitiveLine.CreateCircle(12,20);
            primitiveLine.Render(spriteBatch);
        }
    }
}
