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

        private Dictionary<char, List<Vertices>> _charMapping;
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
            try
            {
                _skeletClient.EndConnect(ar);
                ThreadPool.QueueUserWorkItem(SkeletonsRecieving);
            }
            catch (SocketException ex)
            {
                Debug.WriteLine("Fehler beim Verbinden zum Host");
                Debug.WriteLine(ex);
            }
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
            _charMapping = new Dictionary<char, List<Vertices>>();

            LoadAlphabet();
            LoadNumbers();
            LoadChars();

            //letterMapping.Add(' ', listArray[26]);

            //call method with string you want to print
            //var builder = new StringBuilder();
            //builder.AppendLine("Franz(Peter) jagt im komplett verwahrlosten Taxi quer durch Bayern!?\n1234567890");
            //var dat = DateTime.Now;
            //builder.AppendFormat("{0}:{1} {2}.{3}.{4}", dat.Hour, dat.Minute, dat.Day, dat.Month, dat.Year);
            //BuildText(builder.ToString());

            BuildText(System.IO.File.ReadAllText(System.IO.Path.Combine(ScreenManager.Content.RootDirectory, "maximal.txt")));
            // create sprite based on body
            var rectangle = BodyFactory.CreateRectangle(World, 5f, 1.5f, 1f);
            var shape = rectangle.FixtureList[0].Shape;
            World.RemoveBody(rectangle);
            _obstacle = new Sprite(ScreenManager.Assets.TextureFromShape(shape,
                                                                         MaterialType.Dots,
                                                                         Color.SandyBrown, 0.8f));


        }

        private void LoadAlphabet()
        {
            Texture2D alphabet = ScreenManager.Content.Load<Texture2D>("alphabet");

            uint[] data = new uint[alphabet.Width * alphabet.Height];
            alphabet.GetData(data);

            List<Vertices> list = PolygonTools.CreatePolygon(data, alphabet.Width, 3.5f, 20, true, true);
            Vertices[] listArray = list.ToArray();
            _charMapping.Add('A', GetBreakableParts(listArray[0]));
            _charMapping.Add('B', GetBreakableParts(listArray[2]));
            _charMapping.Add('C', GetBreakableParts(listArray[1]));
            _charMapping.Add('D', GetBreakableParts(listArray[3]));
            _charMapping.Add('E', GetBreakableParts(listArray[4]));
            _charMapping.Add('F', GetBreakableParts(listArray[6]));
            _charMapping.Add('G', GetBreakableParts(listArray[5]));
            _charMapping.Add('H', GetBreakableParts(listArray[7]));
            _charMapping.Add('I', GetBreakableParts(listArray[8]));
            _charMapping.Add('J', GetBreakableParts(listArray[9]));
            _charMapping.Add('K', GetBreakableParts(listArray[10]));
            _charMapping.Add('L', GetBreakableParts(listArray[12]));
            _charMapping.Add('M', GetBreakableParts(listArray[13]));
            _charMapping.Add('N', GetBreakableParts(listArray[14]));
            _charMapping.Add('O', GetBreakableParts(listArray[11]));
            _charMapping.Add('P', GetBreakableParts(listArray[15]));
            _charMapping.Add('Q', GetBreakableParts(listArray[16]));
            _charMapping.Add('R', GetBreakableParts(listArray[19]));
            _charMapping.Add('S', GetBreakableParts(listArray[17]));
            _charMapping.Add('T', GetBreakableParts(listArray[18]));
            _charMapping.Add('U', GetBreakableParts(listArray[20]));
            _charMapping.Add('V', GetBreakableParts(listArray[21]));
            _charMapping.Add('W', GetBreakableParts(listArray[22]));
            _charMapping.Add('X', GetBreakableParts(listArray[23]));
            _charMapping.Add('Y', GetBreakableParts(listArray[24]));
            _charMapping.Add('Z', GetBreakableParts(listArray[25]));
        }

        private void LoadChars()
        {
            Texture2D chars = ScreenManager.Content.Load<Texture2D>("chars");

            uint[] data = new uint[chars.Width * chars.Height];
            chars.GetData(data);

            List<Vertices> list = PolygonTools.CreatePolygon(data, chars.Width, 3.5f, 20, true, true);
            Vertices[] listArray = list.ToArray();
            _charMapping.Add('!', GetBreakableParts(listArray[0]));
            _charMapping.Add('(', GetBreakableParts(listArray[1]));
            _charMapping.Add(')', GetBreakableParts(listArray[2]));
            _charMapping.Add('?', GetBreakableParts(listArray[3]));
            _charMapping.Add(':', GetBreakableParts(listArray[4]));
            _charMapping.Add('.', GetBreakableParts(listArray[5]));
            _charMapping.Add(',', GetBreakableParts(listArray[6]));
        }

        private void LoadNumbers()
        {
            Texture2D numbers = ScreenManager.Content.Load<Texture2D>("numbers");

            uint[] data = new uint[numbers.Width * numbers.Height];
            numbers.GetData(data);

            List<Vertices> list = PolygonTools.CreatePolygon(data, numbers.Width, 3.5f, 20, true, true);
            Vertices[] listArray = list.ToArray();
            _charMapping.Add('0', GetBreakableParts(listArray[0]));
            _charMapping.Add('1', GetBreakableParts(listArray[1]));
            _charMapping.Add('2', GetBreakableParts(listArray[2]));
            _charMapping.Add('3', GetBreakableParts(listArray[3]));
            _charMapping.Add('4', GetBreakableParts(listArray[4]));
            _charMapping.Add('5', GetBreakableParts(listArray[5]));
            _charMapping.Add('6', GetBreakableParts(listArray[6]));
            _charMapping.Add('7', GetBreakableParts(listArray[7]));
            _charMapping.Add('8', GetBreakableParts(listArray[8]));
            _charMapping.Add('9', GetBreakableParts(listArray[9]));
        }

        Dictionary<BreakableBody, Vector2> _letterStartPos = new Dictionary<BreakableBody, Vector2>();


        public void BuildText(string text)
        {
            _text = text;
            char[] textChar = text.ToUpper().ToCharArray();
            float yOffset = -13f;
            float xOffset = -25f;
            _letterStartPos.Clear();

            for (int i = 0; i < textChar.Length; i++)
            {
                char letter = textChar[i];
                if (letter.Equals('\n') || xOffset > 25f || yOffset > 13f)
                {
                    yOffset += 2f;
                    xOffset = -25f;
                }
                List<Vertices> vertices;//letterMapping[letter];
                if (_charMapping.TryGetValue(letter, out vertices))
                {
                    var breakableBodyLetter = new BreakableBody(vertices, World, 1, Guid.NewGuid());
                    breakableBodyLetter.MainBody.Position = new Vector2(xOffset, yOffset);
                    _letterStartPos[breakableBodyLetter] = breakableBodyLetter.MainBody.Position;
                    breakableBodyLetter.Strength = 30;
                    breakableBodyLetter.Parts.ForEach(fixt =>
                    {
                        fixt.CollisionCategories = Category.Cat1;
                        fixt.CollidesWith = fixt.CollisionCategories;
                    });

                    World.AddBreakableBody(breakableBodyLetter);
                    xOffset += 1.5f;
                }
                else if (letter == ' ')
                {
                    xOffset += 1.5f;
                }
            }
        }

        private List<Vertices> GetBreakableParts(Vertices polygon)
        {
            Vector2 centroid = -polygon.GetCentroid();
            polygon.Translate(ref centroid);
            polygon = SimplifyTools.CollinearSimplify(polygon);
            polygon = SimplifyTools.ReduceByDistance(polygon, 4);
            List<Vertices> triangulated = BayazitDecomposer.ConvexPartition(polygon);

            float scale = 0.4f;
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * scale;
            foreach (Vertices vertices in triangulated)
            {
                vertices.Scale(ref vertScale);
            }
            return triangulated;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            foreach (var skeletContainer in TrackedSkelets)
            {
                var skeletJoints = skeletContainer.Joints;
                skeletContainer.CreateRagdoll(this);
                if (skeletJoints == null || skeletJoints.Count == 0) continue;
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

        private void ApplyForce(Body body, SkeletonPoint desiredPoint, float strenght, float toMoveDown, Ragdoll ragdoll)
        {
            var force = (new Vector2(desiredPoint.X, desiredPoint.Y - toMoveDown) - body.Position);
            if (force.Length() > 10f)
            {
                ragdoll.CollisionCategories = Category.Cat1;
                ragdoll.CollidesWith = ragdoll.CollisionCategories;
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

            if (input.IsNewKeyPress(Keys.NumPad1))
            {
                GetTrackedSkelet(0);
            }

            base.HandleInput(input, gameTime);
        }

        private void ResetLetter()
        {
            try
            {
                var keyCollection = _letterStartPos.Keys;
                var breakableBodies = new BreakableBody[keyCollection.Count];
                keyCollection.CopyTo(breakableBodies, 0);
                foreach (var currentLetter in breakableBodies)
                {
                    var letter = currentLetter;
                    var pieces = World.BodyList.FindAll(body => body.UserData != null && body.UserData == letter.MainBody.UserData);
                    var removed = new List<Body>();
                    foreach (var piece in pieces)
                    {
                        if (removed.Contains(piece)) continue;
                        removed.Add(piece);
                        World.RemoveBody(piece);
                    }
                }
                World.ProcessChanges();
                BuildText(_text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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