using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using KinectAddons;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
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
        
        private double _anglesource = 0;
        private SpriteFont _spriteFont;
        private int ScreenWidth { get; set; }
        private Choices choices;
        private SpeechRecognitionEngine speechEngine;
        private String currentText = "";
        private IList<String> allText;


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
            var skeletClient = new TcpClient
                                   {
                                       NoDelay = true
                                   };
            skeletClient.BeginConnect("WS201736", 666, ServerSkeletConnected, skeletClient);
            var audioClient = new TcpClient();
            audioClient.BeginConnect("WS201736", 667, ServerAudioConnected, audioClient);

            allText = new List<String>();
            base.Initialize();
        }

        private void ServerAudioConnected(IAsyncResult ar)
        {
            var tcpClient = ar.AsyncState as TcpClient;
            tcpClient.EndConnect(ar);
            initKinectAudio(tcpClient.GetStream());  
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
            using (var stream = new BufferedStream(client.GetStream()))
            {
                while (client.Connected)
                {
                    var _deserializeJointData = stream.DeserializeJointData();
                    ThreadPool.QueueUserWorkItem(SkeletsRecieved, _deserializeJointData);
                }
            }
        }

        private void SkeletsRecieved(object state)
        {
            var _deserializeJointData = state as TrackedSkelletons;
            if (_deserializeJointData == null)
            {
                return;
            }
            var skellets = _deserializeJointData.Skelletons.Select(skeleton => new Stickman
            {
                Joints = skeleton
            }).ToList();
            _trackedStickmans = skellets;
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


            ScreenWidth = GraphicsDevice.Viewport.TitleSafeArea.Width;
            ScreenHeight = GraphicsDevice.Viewport.TitleSafeArea.Height;
            _trackedStickmans = new List<Stickman>();

            // TODO: use this.Content to load your game content here
        }

        private void initKinectAudio(Stream audioStream)
        {
            RecognizerInfo ri = GetKinectRecognizer();

            speechEngine = new SpeechRecognitionEngine(ri.Id);

            this.initChoices();

            GrammarBuilder gb = new GrammarBuilder();
            gb.Culture = ri.Culture;
            gb.Append(choices);

            var g = new Grammar(gb);

            speechEngine.LoadGrammar(g);
            speechEngine.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(sre_SpeechHypothesized);
            speechEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
            speechEngine.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);

            speechEngine.SetInputToAudioStream(audioStream,
                  new SpeechAudioFormatInfo(
                      EncodingFormat.Pcm, 16000, 16, 1,
                      32000, 2, null));

            speechEngine.RecognizeAsync(RecognizeMode.Multiple);


            //_kinectSensor.AudioSource.SoundSourceAngleChanged += AudioSourceSoundSourceAngleChanged;
        }

        private void initChoices()
        {

            choices = new Choices();
            choices.Add("hello");
            choices.Add("world");
            choices.Add("goodbye");
            choices.Add("my");
            choices.Add("test");
            choices.Add("sentence");
            choices.Add("first");
            choices.Add("exit");
            choices.Add("empty");
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        void sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //System.Diagnostics.Debug.Print("Hypothesized: " + e.Result.Text + " " + e.Result.Confidence);
        }

        void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            //System.Diagnostics.Debug.Print("speech rejected:"+e.Result.Text);
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < 0.7)
            {
                
                System.Diagnostics.Debug.Print(e.Result.Text+" rejected with confidence:"+e.Result.Confidence.ToString());
                return;
            }
            //currentText = e.Result.Text;
            if (e.Result.Text == "empty")
            {
                allText.Clear();
            }
            else
            {
                allText.Add(e.Result.Text);
            }
            string currentSentence = String.Join(" ", allText.ToArray());
            System.Diagnostics.Debug.Print(currentSentence);
            
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
                var stickmans = _trackedStickmans.ToArray();
                stickmans.AsParallel().ForAll(man => man.IsSpeaker = false);
                var t = (from stickman in stickmans
                         let joint = stickman[JointType.Head]
                         let d = Math.Asin(joint.SkeletPoint.X / joint.SkeletPoint.Z) * (180.0 / Math.PI)
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
            var stickmans = _trackedStickmans.ToArray();
            //_trackedStickmans.AsParallel().ForAll(skelet => Drawstickman(skelet, spriteBatch));
            foreach (var trackedSkeleton in stickmans)
            {
                Drawstickman(trackedSkeleton, _spriteBatch);
            }
            DrawSpeech(_spriteBatch, stickmans);
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
            string currentSentence = String.Join(" ", allText.ToArray());
            System.Diagnostics.Debug.Print(currentSentence);
            var measureString = _spriteFont.MeasureString(currentSentence);
            var textWith = measureString.X;
            var textHeight = measureString.Y;
            measureString.Y = 0;
            measureString.X = ScreenWidth / 2f - measureString.X / 2f;
            spriteBatch.DrawString(_spriteFont, currentSentence, measureString, Color.Black);
            var primitiveLine = new PrimitiveLine(GraphicsDevice)
                                    {
                                        Colour = Color.Black,
                                        TransformVector = new Vector2(ScreenWidth / 4, ScreenHeight / 2)
                                    };
            primitiveLine.AddVector(new Vector2(measureString.X + textWith, textHeight + 1) - primitiveLine.TransformVector);
            primitiveLine.AddVector(new Vector2(measureString.X, textHeight + 1) - primitiveLine.TransformVector);
            primitiveLine.AddVector(speaker[JointType.Head].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2).SkeletPoint.Convert());
            primitiveLine.Render(spriteBatch);
        }

        private void DrawDebugStuff(SpriteBatch spriteBatch)
        {
            return;
            int i = 1;
            var msg = String.Format("DiffminAngle:{0}", 0);
            var measureString = _spriteFont.MeasureString(msg);
            measureString.Y = ScreenHeight - measureString.Y * i++;
            measureString.X = ScreenWidth / 2 - measureString.X / 2;
            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.Black);
        }


       

        private void Drawstickman(Stickman trackedSkeleton, SpriteBatch spriteBatch)
        {
            var shoulderCenter = trackedSkeleton[JointType.ShoulderCenter].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var hipCenter = trackedSkeleton[JointType.HipCenter].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var footLeft = trackedSkeleton[JointType.FootLeft].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var footRight = trackedSkeleton[JointType.FootRight].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var handLeft = trackedSkeleton[JointType.HandLeft].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var handRight = trackedSkeleton[JointType.HandRight].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var elbowLeft = trackedSkeleton[JointType.ElbowLeft].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var elbowRight = trackedSkeleton[JointType.ElbowRight].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var kneeLeft = trackedSkeleton[JointType.KneeLeft].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var kneeRight = trackedSkeleton[JointType.KneeRight].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var head = trackedSkeleton[JointType.Head].ScaleOwn(ScreenWidth / 2, ScreenHeight / 2);
            var primitiveLine = new PrimitiveLine(GraphicsDevice)
                                    {
                                        TransformVector = new Vector2(ScreenWidth / 4, ScreenHeight / 2),
                                        Depth = 0
                                    };
            primitiveLine.AddVector(shoulderCenter.SkeletPoint.Convert());
            primitiveLine.AddVector(hipCenter.SkeletPoint.Convert());
            primitiveLine.Colour = trackedSkeleton.BoneColor;
            primitiveLine.Render(spriteBatch);

            primitiveLine.ClearVectors();
            primitiveLine.AddVector(shoulderCenter.SkeletPoint.Convert());
            primitiveLine.AddVector(elbowRight.SkeletPoint.Convert());
            primitiveLine.AddVector(handRight.SkeletPoint.Convert());
            primitiveLine.Render(spriteBatch);
            primitiveLine.ClearVectors();
            primitiveLine.AddVector(shoulderCenter.SkeletPoint.Convert());
            primitiveLine.AddVector(elbowLeft.SkeletPoint.Convert());
            primitiveLine.AddVector(handLeft.SkeletPoint.Convert());
            primitiveLine.Render(spriteBatch);
            primitiveLine.ClearVectors();
            primitiveLine.AddVector(hipCenter.SkeletPoint.Convert());
            primitiveLine.AddVector(kneeRight.SkeletPoint.Convert());
            primitiveLine.AddVector(footRight.SkeletPoint.Convert());
            primitiveLine.Render(spriteBatch);
            primitiveLine.ClearVectors();
            primitiveLine.AddVector(hipCenter.SkeletPoint.Convert());
            primitiveLine.AddVector(kneeLeft.SkeletPoint.Convert());
            primitiveLine.AddVector(footLeft.SkeletPoint.Convert());
            primitiveLine.Render(spriteBatch);

            primitiveLine.ClearVectors();
            primitiveLine.Position = head.SkeletPoint.Convert();
            //primitiveLine.CreateCircle((head.Position.Convert() - shoulderCenter.Position.Convert()).Length(), 20);
            primitiveLine.CreateCircle(12, 20);
            primitiveLine.Render(spriteBatch);
        }
    }
}
