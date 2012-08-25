using System.Collections.Generic;
using System.Text;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KinectInfoScreen
{
    internal class KinectInfoScreen : PhysicsGameScreen, IDemoScreen
    {
        private Border _border;

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
            Dictionary<char,Vertices> letterMapping = new Dictionary<char,Vertices>();
            Vertices[] listArray = list.ToArray();
            letterMapping.Add('A', listArray[0]);
            letterMapping.Add('B', listArray[2]);
            letterMapping.Add('C', listArray[1]);
            letterMapping.Add('D', listArray[3]);
            letterMapping.Add('E', listArray[4]);
            letterMapping.Add('F', listArray[6]);
            letterMapping.Add('G', listArray[5]);
            letterMapping.Add('H', listArray[7]);
            letterMapping.Add('I', listArray[8]);
            letterMapping.Add('J', listArray[9]);
            letterMapping.Add('K', listArray[10]);
            letterMapping.Add('L', listArray[12]);
            letterMapping.Add('M', listArray[13]);
            letterMapping.Add('N', listArray[14]);
            letterMapping.Add('O', listArray[11]);
            letterMapping.Add('P', listArray[15]);
            letterMapping.Add('Q', listArray[16]);
            letterMapping.Add('R', listArray[19]); 
            letterMapping.Add('S', listArray[17]);
            letterMapping.Add('T', listArray[18]);
            letterMapping.Add('U', listArray[20]);
            letterMapping.Add('V', listArray[21]);
            letterMapping.Add('W', listArray[22]);
            letterMapping.Add('X', listArray[23]);
            letterMapping.Add('Y', listArray[24]);
            letterMapping.Add('Z', listArray[25]);
            
            //letterMapping.Add(' ', listArray[26]);
               
            //call method with string you want to print
            this.buildText(letterMapping, "AAAAAAAAAAAAAAAAAA\nAAAAAAAAAAAAAAAAAAAA\nAAAAAAAAAAAAAAA".ToUpper());
           
               
           
        }


        public void buildText(Dictionary<char,Vertices> letterMapping,string text)
        {
            char[] textChar = text.ToCharArray();
            float yOffset = -5f;
            float xOffset = -14f;

            yOffset = 0f;
            xOffset = -14f;

            for (int i = 0; i < textChar.Length;i++)
            {
                char letter = textChar[i];


                Vertices polygon;//letterMapping[letter];
                
                
                if (letterMapping.TryGetValue(letter,out polygon))
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

                    BreakableBody breakableBody = new BreakableBody(triangulated, World, 1);
                    breakableBody.MainBody.Position = new Vector2(xOffset, yOffset);
                    breakableBody.Strength = 100;
                    World.AddBreakableBody(breakableBody);
                }
                if (letter.Equals('\n'))
                {
                    yOffset += 5f;
                    xOffset = -14f;


                }

                xOffset += 3.5f;
            }
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

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _border.Draw();
            base.Draw(gameTime);
        }

        public override void UnloadContent()
        {
            DebugView.RemoveFlags(DebugViewFlags.Shape);
            
            base.UnloadContent();
        }
    }
}