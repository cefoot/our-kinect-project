using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using KinectAddons;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace KinectRotater
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        private const float RotateStep = 0.05f;
        private const float MinHandDistance = 0.5f;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Coordinate Nearest { get; set; }

        private bool Touched { get; set; }

        private Vector3 RelativeLeftHand { get; set; }



        private float _minF = float.MaxValue;

        private float _maxF = float.MinValue;

        private bool _isDebug;
        protected bool IsDebug
        {
            get { return _isDebug; }
            set
            {
                _isDebug = value;
            }
        }

        protected bool IsDebugPressed { get; set; }

        readonly Coordinate[, ,] _coords = new Coordinate[11, 11, 11];
        private SpriteFont _spriteFont;

        static readonly Random Rand = new Random();

        private float _rotX;

        private float _rotY;

        private float _rotZ;
        private Matrix _toNullMatrix;
        private Matrix _toMiddleMatrix;

        protected TransferableJoint RightHand { get; set; }

        protected TransferableJoint LeftHand { get; set; }

        private class Coordinate
        {
            public Coordinate(Vector3 position, Vector3 speed)
            {
                Position = position;
                Speed = speed;
            }
            public Vector3 Position { get; set; }
            public Vector3 Speed { get; set; }

            /// <summary>
            /// Bestimmt, ob das angegebene <see cref="T:System.Object"/> und das aktuelle <see cref="T:System.Object"/> gleich sind.
            /// </summary>
            /// <returns>
            /// true, wenn das angegebene <see cref="T:System.Object"/> gleich dem aktuellen <see cref="T:System.Object"/> ist, andernfalls false.
            /// </returns>
            /// <param name="obj">Das <see cref="T:System.Object"/>, das mit dem aktuellen <see cref="T:System.Object"/> verglichen werden soll. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                var coordinate = obj as Coordinate;
                if (coordinate == null) return false;
                return Position.Equals(coordinate.Position);
            }
        }

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
            InitQube();
            var client = new TcpClient
            {
                NoDelay = true
            };
            client.BeginConnect("WS201736", 666, ServerSkeletConnected, client);
            _toNullMatrix = Matrix.CreateTranslation(-400, -200, -150);
            _toMiddleMatrix = Matrix.CreateTranslation(400, 200, 150);
            base.Initialize();
        }
        #region recieve skelet
        private void ServerSkeletConnected(IAsyncResult ar)
        {
            try
            {
                var tcpClient = ar.AsyncState as TcpClient;
                tcpClient.EndConnect(ar);
                ThreadPool.QueueUserWorkItem(SkeletonsRecieving, tcpClient);
            }
            catch
            {
            }
        }

        private void SkeletonsRecieving(object state)
        {
            var client = state as TcpClient;
            using (var stream = new BufferedStream(client.GetStream()))
            {
                while (client.Connected)
                {
                    var deserializeJointData = stream.DeserializeJointData();
                    var found = false;
                    foreach (var transferableJoint in deserializeJointData.Skelletons.SelectMany(transferableJoints => transferableJoints))
                    {
                        found = true;
                        switch (transferableJoint.JointType)
                        {
                            case JointType.HandLeft:
                                LeftHand = transferableJoint;
                                break;
                            case JointType.HandRight:
                                RightHand = transferableJoint;
                                break;
                        }
                    }
                    if (found) continue;
                    LeftHand = null;
                    RightHand = null;
                }
            }
        }

        #endregion
        const float QubeLengthCnt = 11;
        const int QubePixDist = 20;

        private void InitQube()
        {
            _rotX = 0;
            _rotY = 0;
            _rotZ = 0;
            //middle {X:400 Y:200 Z:150}
            var x = 300;
            for (var x1 = 0; x1 < QubeLengthCnt; x1++)
            {
                var y = 100;
                for (var y1 = 0; y1 < QubeLengthCnt; y1++)
                {
                    var z = 50;
                    for (var z1 = 0; z1 < QubeLengthCnt; z1++)
                    {
                        _coords[x1, y1, z1] = new Coordinate(new Vector3(x, y, z), new Vector3(0, 0, 0));
                        z += QubePixDist;
                    }
                    y += QubePixDist;
                }
                x += QubePixDist;
            }
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
            //circleTxtre = new PrimitiveLine(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected float HandDist { get; set; }
        //private PrimitiveLine circleTxtre;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            var circleTxtre = new PrimitiveLine(GraphicsDevice);
            foreach (var coord in _coords)
            {
                if (coord == null) continue;
                circleTxtre.Color = GetColor(coord);
                circleTxtre.ClearVectors();
                circleTxtre.Position = new Vector2(coord.Position.X, coord.Position.Y);
                circleTxtre.CreateCircle(9, 4, false);
                if (_coords[5, 5, 5].Equals(coord))
                {
                    circleTxtre.CreateCircle(8, 6, false);
                    circleTxtre.CreateCircle(7, 6, false);
                    circleTxtre.CreateCircle(6, 6, false);
                    circleTxtre.CreateCircle(5, 6, false);
                    circleTxtre.CreateCircle(4, 6, false);
                    circleTxtre.CreateCircle(3, 6, false);
                    circleTxtre.CreateCircle(2, 6, false);
                    circleTxtre.CreateCircle(1, 6, false);
                }
                circleTxtre.Render(spriteBatch);
            }
            DrawHand();
            DrawDebug(gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHand()
        {
            if (LeftHand == null || RightHand == null) return;
            if (HandDist > MinHandDistance)
            {
                DrawHand(RelativeLeftHand, false);
                DrawHand(-RelativeLeftHand, false);
            }
            else
            {
                DrawHand(RelativeLeftHand, true);
                DrawHand(-RelativeLeftHand, true);
            }
        }

        private void DrawHand(Vector3 relativePos, bool fill)
        {
            var position = _coords[5, 5, 5].Position;
            position += relativePos * 150;
            var circle = new PrimitiveLine(GraphicsDevice)
            {
                Color = Color.ForestGreen,
                Position = new Vector2(position.X, position.Y)
            };

            circle.CreateCircle(9, 4, false);
            if (fill)
            {
                circle.CreateCircle(8, 6, false);
                circle.CreateCircle(7, 6, false);
                circle.CreateCircle(6, 6, false);
                circle.CreateCircle(5, 6, false);
                circle.CreateCircle(4, 6, false);
                circle.CreateCircle(3, 6, false);
                circle.CreateCircle(2, 6, false);
                circle.CreateCircle(1, 6, false);
            }
            circle.Render(spriteBatch);
        }

        private void DrawDebug(GameTime gameTime)
        {
            if (!IsDebug) return;
            var fps = 1f / gameTime.ElapsedGameTime.TotalSeconds;
            var y = 0;
            spriteBatch.DrawString(_spriteFont, "MaxF:" + _maxF, new Vector2(0, 20 * y++), Color.White);
            spriteBatch.DrawString(_spriteFont, "MinF:" + _minF, new Vector2(0, 20 * y++), Color.White);
            spriteBatch.DrawString(_spriteFont, "FPS:" + fps, new Vector2(0, 20 * y++), Color.White);
            spriteBatch.DrawString(_spriteFont, "RotX:" + _rotX, new Vector2(0, 20 * y++), Color.White);
            spriteBatch.DrawString(_spriteFont, "RotY:" + _rotY, new Vector2(0, 20 * y++), Color.White);
            spriteBatch.DrawString(_spriteFont, "RotZ:" + _rotZ, new Vector2(0, 20 * y++), Color.White);
            spriteBatch.DrawString(_spriteFont, "HandL:" + (LeftHand == null ? "" : LeftHand.SkeletPoint.ToString()), new Vector2(0, 20 * y++), Color.White);
            spriteBatch.DrawString(_spriteFont, "HandR:" + (RightHand == null ? "" : RightHand.SkeletPoint.ToString()), new Vector2(0, 20 * y++), Color.White);
            spriteBatch.DrawString(_spriteFont, "HandDist:" + HandDist, new Vector2(0, 20 * y++), Color.White);
        }

        private Color GetColor(Coordinate coord)
        {
            if (coord.Equals(_coords[5, 5, 5])) return Color.Red;
            if (coord.Equals(Nearest)) return Color.Salmon;
            var clr = Color.White;
            var f = coord.Position.Z - _coords[5, 5, 5].Position.Z;
            f *= 1.2f;
            f += 128f;
            clr.A = (byte)f;
            clr.R = (byte)f;
            _minF = _minF > f ? f : _minF;
            _maxF = _maxF < f ? f : _maxF;
            return clr;
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

            var newState = Keyboard.GetState();
            ProcessHands();
            Rotate(newState.GetPressedKeys());
            AutoRotate();
            EnableDebug(newState);

            base.Update(gameTime);
        }

        private void ProcessHands()
        {
            if (LeftHand == null) return;
            var leftVec = new Vector3(LeftHand.SkeletPoint.X, LeftHand.SkeletPoint.Y, LeftHand.SkeletPoint.Z);
            var rightVec = new Vector3(RightHand.SkeletPoint.X, RightHand.SkeletPoint.Y, RightHand.SkeletPoint.Z);
            var distance = leftVec - rightVec;
            HandDist = distance.Length();
            distance *= 0.5f;
            distance += rightVec;
            var addRight = rightVec - distance;
            var addLeft = leftVec - distance;
            addRight.Normalize();
            addLeft.Normalize();
            RelativeLeftHand = addLeft;
            if (HandDist < MinHandDistance)
            {
                _rotX = 0;
                _rotY = 0;
                _rotZ = 0;
                Touch(RelativeLeftHand);
                UpdateTouched(RelativeLeftHand);
            }
            else
            {
                Touched = false;
            }
        }

        private void UpdateTouched(Vector3 relativeLeftHand)
        {
            MoveQubeToNull();

            var position = _coords[5, 5, 5].Position;
            var vectorToStartPoint = Nearest.Position - position;
            var angleXYToStart = Correct(Math.Atan2(vectorToStartPoint.Y, vectorToStartPoint.X));
             var angleXYToHand = Correct(Math.Atan2(relativeLeftHand.Y, relativeLeftHand.X));
            _rotZ = angleXYToHand - angleXYToStart;
            //_rotX = Correct(Math.Atan2(vectorToStartPoint.Y, vectorToStartPoint.Z)) - Correct(Math.Atan2(relativeLeftHand.Y, relativeLeftHand.Z));
            _rotY = Correct(Math.Atan2(relativeLeftHand.X, relativeLeftHand.Z))- Correct(Math.Atan2(vectorToStartPoint.X, vectorToStartPoint.Z));
            MoveQubeToMiddle();
        }

        private float Correct(double atan2)
        {
            if(atan2 < 0)
            {
                atan2 += Math.PI*2;
            }
            return (float) atan2;
        }

        private void MoveQubeToMiddle()
        {
            var translation = _toMiddleMatrix;// Matrix.CreateTranslation(400, 200, 150 - HandDist * 10);
            foreach (var coordinate in _coords)
            {
                coordinate.Position = Vector3.Transform(coordinate.Position, translation);
            }
        }

        private void MoveQubeToNull()
        {
            foreach (var coordinate in _coords)
            {
                coordinate.Position = Vector3.Transform(coordinate.Position, _toNullMatrix);
            }
        }

        private static float CalculateRot(Vector2 from, Vector2 to)
        {
            from.Normalize();
            to.Normalize();
            var length = (from - to).Length();
            var alpha = Math.Asin(length / 2);
            var b = alpha * ((2 * Math.PI) / 180);
            return (float)b;


            //return (float) Math.Acos(Vector2.Dot(from, to));
        }

        private void Touch(Vector3 relativePos)
        {
            if (Touched) return;
            var absolutHandPos = _coords[5, 5, 5].Position + relativePos * 150;
            var min = float.MaxValue;
            foreach (var coordinate in _coords)
            {
                var dist = (absolutHandPos - coordinate.Position).Length();
                if (dist >= min) continue;
                min = dist;
                Nearest = coordinate;
            }
            Touched = true;
        }

        private void AutoRotate()
        {
            _rotX = UpdateRotation(_rotX);
            _rotY = UpdateRotation(_rotY);
            _rotZ = UpdateRotation(_rotZ);
            MoveQubeToNull();
            RotateRoundX();
            RotateRoundY();
            RotateRoundZ();
            MoveQubeToMiddle();
        }

        private static float UpdateRotation(float rot)
        {
            if (rot == 0) return rot;
            rot *= 0.1f;
            return rot;
        }

        private void EnableDebug(KeyboardState newState)
        {
            if (IsDebugPressed && newState.IsKeyUp(Keys.D))
            {
                IsDebugPressed = false;
                IsDebug = !IsDebug;
            }
            if (newState.IsKeyDown(Keys.D))
            {
                IsDebugPressed = true;
            }
        }

        private void Rotate(Keys[] pressedKeys)
        {
            if (pressedKeys.Contains(Keys.Left))
            {
                _rotY = RotateStep;
                //RotateRoundY();
            }
            if (pressedKeys.Contains(Keys.Right))
            {
                _rotY = -RotateStep;
                //RotateRoundY();
            }
            if (pressedKeys.Contains(Keys.Up))
            {
                _rotX = RotateStep;
                //RotateRoundX();
            }
            if (pressedKeys.Contains(Keys.Down))
            {
                _rotX = -RotateStep;
                //RotateRoundX();
            }
            if (pressedKeys.Contains(Keys.R))
            {
                InitQube();
            }
            if (pressedKeys.Contains(Keys.T))
            {
                _rotX = RotateStep * (float)Rand.Next(10, 100);
                _rotY = RotateStep * (float)Rand.Next(10, 100);
                _rotZ = RotateStep * (float)Rand.Next(10, 100);
            }
        }

        private void RotateRoundZ()
        {
            if (_rotZ == 0) return;
            var rotation = Matrix.CreateRotationZ(_rotZ);
            foreach (var coord in _coords)
            {
                coord.Position = Vector3.Transform(coord.Position, rotation);
            }
        }

        private void RotateRoundY()
        {
            if (_rotY == 0) return;
            var rotation = Matrix.CreateRotationY(_rotY);
            foreach (var coord in _coords)
            {
                coord.Position = Vector3.Transform(coord.Position, rotation);
            }
        }

        private void RotateRoundX()
        {
            if (_rotX == 0) return;
            var rotation = Matrix.CreateRotationX(_rotX);
            foreach (var coord in _coords)
            {
                coord.Position = Vector3.Transform(coord.Position, rotation);
            }
        }
    }

    #region DRAW

    #endregion

}
