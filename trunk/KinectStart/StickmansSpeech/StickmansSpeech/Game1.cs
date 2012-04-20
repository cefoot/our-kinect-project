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
        SpriteBatch _spriteBatch;
        private IList<Stickman> _trackedStickmans;
        private KinectSensor _kinectSensor;
        private double _anglesource;
        private SpriteFont _spriteFont;
        private int ScreenWidth { get; set; }

        private int ScreenHeight { get; set; }

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
            foreach (var stickman in skeletsData.Select(skeleton => new Stickman
            {
                Skeleton = skeleton
            }))
            {
                if(stickman.Skeleton.TrackingState != SkeletonTrackingState.Tracked)
                {
                    _trackedStickmans.Remove(stickman);
                }else if(!_trackedStickmans.Contains(stickman))
                {
                    _trackedStickmans.Add(stickman);
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("Arial");

            _kinectSensor.AudioSource.Start();

            _kinectSensor.AudioSource.SoundSourceAngleChanged += AudioSourceSoundSourceAngleChanged;
            ScreenWidth = GraphicsDevice.Viewport.TitleSafeArea.Width;
            ScreenHeight = GraphicsDevice.Viewport.TitleSafeArea.Height;
            _trackedStickmans = new List<Stickman>();

            // TODO: use this.Content to load your game content here
        }

        void AudioSourceSoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            _anglesource = e.ConfidenceLevel > .4 ? e.Angle : 1000;
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
            if (_trackedStickmans != null && _anglesource != 1000)
            {
                _trackedStickmans.AsParallel().ForAll(man => man.IsSpeaker = false);
                var t = (from stickman in _trackedStickmans
                         let joint = stickman.Skeleton.Joints[JointType.Head]
                         let d = Math.Asin(joint.Position.X/joint.Position.Z) * (180.0 / Math.PI)
                         select new Tuple<Stickman, double>(stickman, Math.Abs(d - _anglesource)));
                if (t.Count() > 0)
                {
                    var minAngleDiff = t.Min(tupl => tupl.Item2);
                    if (minAngleDiff < 15d)
                    {
                        var speaker = t.First(tupl => tupl.Item2 == minAngleDiff);
                        speaker.Item1.IsSpeaker = true;
                    }
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice.Clear(Color.White);
            _spriteBatch.Begin();
            //_trackedStickmans.AsParallel().ForAll(skelet => Drawstickman(skelet, spriteBatch));
            foreach (var trackedSkeleton in _trackedStickmans)
            {

                Drawstickman(trackedSkeleton, _spriteBatch);
            }
            DrawSpeech(_spriteBatch,_trackedStickmans);
            DrawDebugStuff(_spriteBatch);
            // TODO: Add your drawing code here
            _spriteBatch.End();
            //base.Draw(gameTime);
        }

        private void DrawSpeech(SpriteBatch spriteBatch, IEnumerable<Stickman> trackedSkeletons)
        {
            var speaker = trackedSkeletons.FirstOrDefault(skel => skel.IsSpeaker);
            if (speaker == null)
            {
                return;
            }
            const string msg = "Bla..";
            var measureString = _spriteFont.MeasureString(msg);
            var textWith = measureString.X;
            var textHeight = measureString.Y;
            measureString.Y = 0;
            measureString.X = ScreenWidth / 2f - measureString.X / 2f;
            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.Black);
            var primitiveLine = new PrimitiveLine(GraphicsDevice)
                                    {
                                        Colour = Color.Black,
                                        TransformVector = new Vector2(ScreenWidth/4, ScreenHeight/2)
                                    };
            primitiveLine.AddVector(new Vector2(measureString.X + textWith, textHeight + 1) - primitiveLine.TransformVector);
            primitiveLine.AddVector(new Vector2(measureString.X, textHeight + 1) - primitiveLine.TransformVector);
            primitiveLine.AddVector(speaker.Skeleton.Joints[JointType.Head].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2).Position.Convert());
            primitiveLine.Render(spriteBatch);
        }

        private void DrawDebugStuff(SpriteBatch spriteBatch)
        {
            return;
            int i = 1;
            var msg = String.Format("DiffminAngle:{0}", 0);
            var measureString = _spriteFont.MeasureString(msg);
            measureString.Y = ScreenHeight - measureString.Y*i++;
            measureString.X = ScreenWidth/2 - measureString.X/2;
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

        private void Drawstickman(Stickman trackedSkeleton, SpriteBatch spriteBatch)
        {
            var shoulderCenter = trackedSkeleton.Skeleton.Joints[JointType.ShoulderCenter].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var hipCenter = trackedSkeleton.Skeleton.Joints[JointType.HipCenter].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var footLeft = trackedSkeleton.Skeleton.Joints[JointType.FootLeft].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var footRight = trackedSkeleton.Skeleton.Joints[JointType.FootRight].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var handLeft = trackedSkeleton.Skeleton.Joints[JointType.HandLeft].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var handRight = trackedSkeleton.Skeleton.Joints[JointType.HandRight].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var elbowLeft = trackedSkeleton.Skeleton.Joints[JointType.ElbowLeft].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var elbowRight = trackedSkeleton.Skeleton.Joints[JointType.ElbowRight].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var kneeLeft = trackedSkeleton.Skeleton.Joints[JointType.KneeLeft].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var kneeRight = trackedSkeleton.Skeleton.Joints[JointType.KneeRight].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var head = trackedSkeleton.Skeleton.Joints[JointType.Head].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var primitiveLine = new PrimitiveLine(GraphicsDevice)
                                    {
                                        TransformVector = new Vector2(ScreenWidth/4, ScreenHeight/2),
                                        Depth = 0
                                    };
            primitiveLine.AddVector(shoulderCenter.Position.Convert());
            primitiveLine.AddVector(hipCenter.Position.Convert());
            primitiveLine.Colour = trackedSkeleton.BoneColor;
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
