using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using KinectAddons;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KinectInfoScreen
{
    internal class KinectInfoScreen : PhysicsGameScreen, IDemoScreen
    {
        private Border _border;

        private TcpClient _skeletClient;

        private List<SkeletContainer> TrackedSkelets { get; set; }

        private int _triangleCount = -2;
        private Sprite _obstacle;
        private const float BaseLine = 24;

        private Dictionary<char, Vertices> _letterMapping;
        private string _text;

        private float _force = 1000f;

        public KinectInfoScreen()
        {
            _skeletClient = new TcpClient { NoDelay = true };
            _skeletClient.BeginConnect("ws201736", 666, ServerConnected, null);
            TrackedSkelets = new List<SkeletContainer>();
        }

        #region skelet recieving

        private void ServerConnected(IAsyncResult ar)
        {
            _skeletClient.EndConnect(ar);
            ThreadPool.QueueUserWorkItem(SkeletonsRecieving);
        }
        private void SkeletonsRecieving(object state)
        {
            var networkStream = _skeletClient.GetStream();
            while (_skeletClient.Connected)
            {
                var deserializeJointData = networkStream.DeserializeJointData();
                ThreadPool.QueueUserWorkItem(SkeletsRecieved, deserializeJointData);
            }
        }

        private void SkeletsRecieved(object state)
        {
            var deserializeJointData = state as TrackedSkelletons;
            if (deserializeJointData == null)
            {
                return;
            }
            var skeletIdx = 0;
            foreach (var skelets in deserializeJointData.Skelletons)//.AsParallel().Select(skelleton => skelleton.First(jnt => jnt.JointType == TypeOfJoint)).Select(transferableJoint => transferableJoint.SkeletPoint.ScaleOwn(80, 60)))
            {
                lock (TrackedSkelets)
                {
                    var skelet = new Dictionary<JointType, SkeletonPoint>();
                    foreach (var transferableJoint in skelets)
                    {
                        skelet[transferableJoint.JointType] = new SkeletonPoint
                        {
                            X =
                                transferableJoint.SkeletPoint.X * 30f,
                            Y =
                            transferableJoint.SkeletPoint.Y * -20f,
                        };
                    }
                    GetTrackedSkelet(skeletIdx, skelet).Joints = skelet;
                    GetTrackedSkelet(skeletIdx).UpdateDeleteTimer();
                    skeletIdx++;
                }
            }
        }

        private SkeletContainer GetTrackedSkelet(int skeletIdx, Dictionary<JointType, SkeletonPoint> skelet = null)
        {
            while (TrackedSkelets.Count < skeletIdx + 1)
            {
                var skeletContainer = new SkeletContainer(World);
                if (skelet != null)
                {
                    skeletContainer.Joints = skelet;
                }
                TrackedSkelets.Add(skeletContainer);
            }
            return TrackedSkelets[skeletIdx];
        }

        #endregion

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Breakable bodies and explosions";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Explode (at cursor): B button");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Explode (at cursor): Right click");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            DebugView.AppendFlags(DebugViewFlags.Shape);

            World.Gravity = Vector2.Zero;

            _border = new Border(World, this, ScreenManager.GraphicsDevice.Viewport);

            Texture2D alphabet = ScreenManager.Content.Load<Texture2D>("alphabet");

            uint[] data = new uint[alphabet.Width * alphabet.Height];
            alphabet.GetData(data);

            List<Vertices> list = PolygonTools.CreatePolygon(data, alphabet.Width, 3.5f, 20, true, true);
            _letterMapping = new Dictionary<char, Vertices>();
            Vertices[] listArray = list.ToArray();
            _letterMapping.Add('A', listArray[0]);
            _letterMapping.Add('B', listArray[2]);
            _letterMapping.Add('C', listArray[1]);
            _letterMapping.Add('D', listArray[3]);
            _letterMapping.Add('E', listArray[4]);
            _letterMapping.Add('F', listArray[6]);
            _letterMapping.Add('G', listArray[5]);
            _letterMapping.Add('H', listArray[7]);
            _letterMapping.Add('I', listArray[8]);
            _letterMapping.Add('J', listArray[9]);
            _letterMapping.Add('K', listArray[10]);
            _letterMapping.Add('L', listArray[12]);
            _letterMapping.Add('M', listArray[13]);
            _letterMapping.Add('N', listArray[14]);
            _letterMapping.Add('O', listArray[11]);
            _letterMapping.Add('P', listArray[15]);
            _letterMapping.Add('Q', listArray[16]);
            _letterMapping.Add('R', listArray[19]);
            _letterMapping.Add('S', listArray[17]);
            _letterMapping.Add('T', listArray[18]);
            _letterMapping.Add('U', listArray[20]);
            _letterMapping.Add('V', listArray[21]);
            _letterMapping.Add('W', listArray[22]);
            _letterMapping.Add('X', listArray[23]);
            _letterMapping.Add('Y', listArray[24]);
            _letterMapping.Add('Z', listArray[25]);

            //letterMapping.Add(' ', listArray[26]);

            //call method with string you want to print
            BuildText("Hello  World");


            // create sprite based on body
            var rectangle = BodyFactory.CreateRectangle(World, 5f, 1.5f, 1f);
            var shape = rectangle.FixtureList[0].Shape;
            World.RemoveBody(rectangle);
            _obstacle = new Sprite(ScreenManager.Assets.TextureFromShape(shape,
                                                                         MaterialType.Dots,
                                                                         Color.SandyBrown, 0.8f));


        }

        Dictionary<BreakableBody, Vector2> _letterStartPos = new Dictionary<BreakableBody, Vector2>();


        public void BuildText(string text)
        {
            _text = text;
            char[] textChar = text.ToUpper().ToCharArray();
            float yOffset = -5f;
            float xOffset = -14f;

            yOffset = 0f;
            xOffset = -18f;
            _letterStartPos.Clear();

            for (int i = 0; i < textChar.Length; i++)
            {
                char letter = textChar[i];


                Vertices polygon;//letterMapping[letter];


                if (_letterMapping.TryGetValue(letter, out polygon))
                {
                    Vector2 centroid = -polygon.GetCentroid();
                    polygon.Translate(ref centroid);
                    polygon = SimplifyTools.CollinearSimplify(polygon);
                    polygon = SimplifyTools.ReduceByDistance(polygon, 4);
                    List<Vertices> triangulated = BayazitDecomposer.ConvexPartition(polygon);

                    float scale = 1.0f;
                    Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * scale;
                    foreach (Vertices vertices in triangulated)
                    {
                        vertices.Scale(ref vertScale);
                    }

                    var breakableBodyLetter = new BreakableBody(triangulated, World, 1);
                    breakableBodyLetter.MainBody.Position = new Vector2(xOffset, yOffset);
                    _letterStartPos[breakableBodyLetter] = new Vector2(xOffset, yOffset);
                    breakableBodyLetter.Strength = 100;
                    breakableBodyLetter.Parts.ForEach(fixt =>
                    {
                        fixt.CollisionCategories = Category.Cat1;
                        fixt.CollidesWith = Category.Cat1;
                    });

                    World.AddBreakableBody(breakableBodyLetter);
                    //           _currentUsedLetters.Add(breakableBodyLetter);
                }
                if (letter.Equals('\n'))
                {
                    yOffset += 5f;
                    xOffset = -14f;


                }
                xOffset += 3.5f;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            foreach (var skeletContainer in TrackedSkelets)
            {
                var skeletJoints = skeletContainer.Joints;
                skeletContainer.CreateRagdoll(this);
                UpdateRagdoll(skeletContainer.Ragdoll, skeletJoints);

            }
            //ragdoll.Body.Position
        }

        private void UpdateRagdoll(Ragdoll ragdoll, Dictionary<JointType, SkeletonPoint> skeletonPoint)
        {
            //ragdoll.Body.Position = new Vector2(skeletonPoint.X, skeletonPoint.Y);
            var toMoveDown = 0;// _groundlines[skeletIdx] - BaseLine;
            ApplyForce(ragdoll.Body, skeletonPoint[JointType.Spine], _force, toMoveDown, ragdoll);
            var strenghtExt = _force / 30f;
            ApplyForce(ragdoll.LeftFoot, skeletonPoint[JointType.FootLeft], strenghtExt, toMoveDown, ragdoll);
            ApplyForce(ragdoll.RightFoot, skeletonPoint[JointType.FootRight], strenghtExt, toMoveDown, ragdoll);
            ApplyForce(ragdoll.LeftHand, skeletonPoint[JointType.HandLeft], strenghtExt, toMoveDown, ragdoll);
            ApplyForce(ragdoll.RightHand, skeletonPoint[JointType.HandRight], strenghtExt, toMoveDown, ragdoll);
            ApplyForce(ragdoll.Head, skeletonPoint[JointType.Head], strenghtExt, toMoveDown, ragdoll);
        }

        private void ApplyForce(Body body, SkeletonPoint skeletonPoint, float strenght, float toMoveDown, Ragdoll ragdoll)
        {
            var force = (new Vector2(skeletonPoint.X, skeletonPoint.Y - toMoveDown) - body.Position);
            if (force.Length() > 10f)
            {
                ragdoll.CollisionCategories = Category.Cat1;
                ragdoll.CollidesWith = Category.Cat1;
            }
            body.ApplyForce(strenght * force);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewMouseButtonPress(MouseButtons.RightButton) ||
                input.IsNewButtonPress(Buttons.B))
            {
                Vector2 cursorPos = Camera.ConvertScreenToWorld(input.Cursor);

                Vector2 min = cursorPos - new Vector2(10, 10);
                Vector2 max = cursorPos + new Vector2(10, 10);

                AABB aabb = new AABB(ref min, ref max);

                World.QueryAABB(fixture =>
                                    {
                                        Vector2 fv = fixture.Body.Position - cursorPos;
                                        fv.Normalize();
                                        fv *= 40;
                                        fixture.Body.ApplyLinearImpulse(ref fv);
                                        return true;
                                    }, ref aabb);
            }

            if (input.IsNewKeyPress(Keys.R))
            {
                ResetLetter();
            }

            base.HandleInput(input, gameTime);
        }

        private void ResetLetter()
        {
            foreach (var currentLetter in _letterStartPos)
            {
                currentLetter.Key.Broken = false;
                currentLetter.Key.MainBody.Position = new Vector2(currentLetter.Value.X, currentLetter.Value.Y);
                //BuildText(_text);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _border.Draw();
            DrawSkelet();
            base.Draw(gameTime);
        }

        private void DrawSkelet()
        {
            ScreenManager.SpriteBatch.Begin(0, null, null, null, null, null, Camera.View);
            foreach (var skelet in TrackedSkelets)
            {
                if (skelet.Ragdoll == null) continue;
                skelet.Ragdoll.Draw();
            }
            ScreenManager.SpriteBatch.End();
        }

        public override void UnloadContent()
        {
            DebugView.RemoveFlags(DebugViewFlags.Shape);
            base.UnloadContent();
        }

        private List<VertexPositionColor> GetTriangleStrip(Vector3[] points, float thickness)
        {
            var lastPoint = Vector3.Zero;
            var list = new List<VertexPositionColor>();
            for (var i = 0; i < points.Length; i++)
            {
                if (i == 0) { lastPoint = points[i]; continue; }
                //the direction of the current line
                var direction = lastPoint - points[i];
                direction.Normalize();
                //the perpendiculat to the current line
                var normal = Vector3.Cross(direction, Vector3.UnitZ);
                normal.Normalize();
                var p1 = lastPoint + normal * thickness; _triangleCount++;
                var p2 = lastPoint - normal * thickness; _triangleCount++;
                var p3 = points[i] + normal * thickness; _triangleCount++;
                var p4 = points[i] - normal * thickness; _triangleCount++;
                list.Add(new VertexPositionColor(p1, Color.Black));
                list.Add(new VertexPositionColor(p2, Color.Black));
                list.Add(new VertexPositionColor(p3, Color.Black));
                list.Add(new VertexPositionColor(p4, Color.Black));
                lastPoint = points[i];
            }
            return list;
        }
    }
}