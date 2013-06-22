using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace PixelQube
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Pixel[, ,] Qube = new Pixel[11, 11, 11];
        Pixel Left { get; set; }
        Pixel Right { get; set; }
        ISet<Link> LinkList = new HashSet<Link>();
        float _fps = 1f;
        Vector3 CamPos { get; set; }
        Vector3 LookAt { get; set; }
        readonly Vector3 StartCamPos = new Vector3(5, 5, 180);
        readonly Plane Ground = new Plane(new Vector3(-100, -50, 100), new Vector3(100, -50, -100), new Vector3(100, -50, 100));
        Vector3 Gravity { get; set; }
        float FPS                         
        {                                 
            get { return _fps; }
            set
            {
                _fps = value;
                TargetElapsedTime = TimeSpan.FromSeconds(1f / FPS);
                Debug.WriteLine(_fps);
            }
        }
        private BasicEffect basicEffect;
        


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            FPS = 60f;
            Gravity = Vector3.Zero;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            CreateForm();


            base.Initialize();
        }

        private void CreateForm()
        {
            LinkList.Clear();
            //BACK
            Qube[0, 0, 0] = new Pixel
            {
                Position = new Vector3(0, 0, 0),
            };
            Qube[10, 0, 0] = new Pixel
            {
                Position = new Vector3(10, 0, 0),
            };
            Qube[0, 10, 0] = new Pixel
            {
                Position = new Vector3(0, 10, 0),
            };
            Qube[10, 10, 0] = new Pixel
            {
                Position = new Vector3(10, 10, 0),
            };
            LinkList.Add(new Link(Qube[0, 0, 0], Qube[10, 0, 0]));
            LinkList.Add(new Link(Qube[0, 0, 0], Qube[0, 10, 0]));
            LinkList.Add(new Link(Qube[10, 10, 0], Qube[0, 0, 0]));
            LinkList.Add(new Link(Qube[10, 0, 0], Qube[0, 10, 0]));
            LinkList.Add(new Link(Qube[10, 10, 0], Qube[10, 0, 0]));
            LinkList.Add(new Link(Qube[10, 10, 0], Qube[0, 10, 0]));
            //FRONT
            Qube[0, 0, 10] = new Pixel
            {
                Position = new Vector3(0, 0, 10),
            };
            Qube[10, 0, 10] = new Pixel
            {
                Position = new Vector3(10, 0, 10),
            };
            Qube[0, 10, 10] = new Pixel
            {
                Position = new Vector3(0, 10, 10),
            };
            Qube[10, 10, 10] = new Pixel
            {
                Position = new Vector3(10, 10, 10),
            };
            LinkList.Add(new Link(Qube[0, 0, 10], Qube[10, 0, 10]));
            LinkList.Add(new Link(Qube[0, 0, 10], Qube[0, 10, 10]));
            LinkList.Add(new Link(Qube[10, 10, 10], Qube[0, 0, 10]));
            LinkList.Add(new Link(Qube[10, 0, 10], Qube[0, 10, 10]));
            LinkList.Add(new Link(Qube[10, 10, 10], Qube[10, 0, 10]));
            LinkList.Add(new Link(Qube[10, 10, 10], Qube[0, 10, 10]));
            //LEFT
            LinkList.Add(new Link(Qube[0, 0, 0], Qube[0, 0, 10]));
            LinkList.Add(new Link(Qube[0, 10, 0], Qube[0, 10, 10]));
            LinkList.Add(new Link(Qube[0, 0, 0], Qube[0, 10, 10]));
            LinkList.Add(new Link(Qube[0, 10, 0], Qube[0, 0, 10]));
            //Right
            LinkList.Add(new Link(Qube[10, 0, 0], Qube[10, 0, 10]));
            LinkList.Add(new Link(Qube[10, 10, 0], Qube[10, 10, 10]));
            LinkList.Add(new Link(Qube[10, 0, 0], Qube[10, 10, 10]));
            LinkList.Add(new Link(Qube[10, 10, 0], Qube[10, 0, 10]));
            //BOTTOM
            LinkList.Add(new Link(Qube[10, 0, 0], Qube[0, 0, 10]));
            LinkList.Add(new Link(Qube[0, 0, 0], Qube[10, 0, 10]));
            //Top
            LinkList.Add(new Link(Qube[10, 10, 0], Qube[0, 10, 10]));
            LinkList.Add(new Link(Qube[0, 10, 0], Qube[10, 10, 10]));
            //Middle
            LinkList.Add(new Link(Qube[0, 0, 0], Qube[10, 10, 10]));
            LinkList.Add(new Link(Qube[0, 0, 10], Qube[10, 10, 0]));
            LinkList.Add(new Link(Qube[0, 10, 10], Qube[10, 0, 0]));
            LinkList.Add(new Link(Qube[10, 0, 10], Qube[0, 10, 0]));

            Left = Qube[0, 0, 0];
            Right = Qube[10, 10, 10];
           
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            basicEffect = new BasicEffect(GraphicsDevice);
            CamPos = StartCamPos;
            LookAt = new Vector3(5, 5, 10);
            basicEffect.View = Matrix.CreateLookAt(CamPos, LookAt, Vector3.Up);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), GraphicsDevice.Viewport.AspectRatio, 1f, 1000f);


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
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Escape))
                this.Exit();
            MoveLeft(Keyboard.GetState().GetPressedKeys());
            MoveRight(Keyboard.GetState().GetPressedKeys());
            RotateCam(Keyboard.GetState().GetPressedKeys());
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.R))
                CreateForm();
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Add) || Keyboard.GetState().GetPressedKeys().Contains(Keys.OemPlus))
                FPS++;
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Subtract) || Keyboard.GetState().GetPressedKeys().Contains(Keys.OemMinus))
                FPS--;
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.G) && Gravity == Vector3.Zero)
                Gravity = new Vector3(0f, -0.2f, 0f);
            else if (Keyboard.GetState().GetPressedKeys().Contains(Keys.G))
                Gravity = Vector3.Zero;
            foreach (var pxl in Qube)
            {
                if (pxl == null) continue;
                pxl.Move(Gravity, Ground);
            }
            foreach (var lnk in LinkList)
            {
                lnk.ApplyForce();
            }

            base.Update(gameTime);
        }

        private void RotateCam(Keys[] keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.NumPad4:
                        CamPos += new Vector3(5, 0, 0);
                        continue;
                    case Keys.NumPad8:
                        CamPos += new Vector3(0, 5, 0);
                        continue;
                    case Keys.NumPad6:
                        CamPos -= new Vector3(5, 0, 0);
                        continue;
                    case Keys.NumPad2:
                        CamPos -= new Vector3(0, 5, 0);
                        continue;
                    case Keys.NumPad9:
                        CamPos -= new Vector3(0, 0, 5);
                        continue;
                    case Keys.NumPad3:
                        CamPos += new Vector3(0, 0, 5);
                        continue;
                    case Keys.NumPad5:
                        CamPos = StartCamPos;
                        continue;
                }
            }
            basicEffect.View = Matrix.CreateLookAt(CamPos, LookAt, Vector3.Up);
        }

        private void MoveRight(Keys[] keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.Left:
                        Right.Speed += new Vector3(-5, 0, 0);
                        continue;
                    case Keys.Right:
                        Right.Speed += new Vector3(5, 0, 0);
                        continue;
                    case Keys.Up:
                        Right.Speed += new Vector3(0, 5, 0);
                        continue;
                    case Keys.Down:
                        Right.Speed += new Vector3(0, -5, 0);
                        continue;
                }
            }
        }

        private void MoveLeft(Keys[] keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.A:
                        Left.Speed += new Vector3(-5, 0, 0);
                        continue;
                    case Keys.D:
                        Left.Speed += new Vector3(5, 0, 0);
                        continue;
                    case Keys.W:
                        Left.Speed += new Vector3(0, 5, 0);
                        continue;
                    case Keys.S:
                        Left.Speed += new Vector3(0, -5, 0);
                        continue;
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            basicEffect.CurrentTechnique.Passes[0].Apply();
            //draw moveable stuff
            foreach (var link in LinkList)
            {
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, new[] { 
                    new VertexPositionColor(link.Pixel1.Position, link.Color), 
                    new VertexPositionColor(link.Pixel2.Position, link.Color) 
                }, 0, 1);
            }
            //draw solid stuff
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, new []{
                new VertexPositionColor(new Vector3(-100,-50,100), Color.Gray),
                new VertexPositionColor(new Vector3(100,-50,-100), Color.Gray),
                new VertexPositionColor(new Vector3(100,-50,100), Color.Gray),
                
                new VertexPositionColor(new Vector3(-100,-50,100), Color.Gray),
                new VertexPositionColor(new Vector3(-100,-50,-100), Color.Gray),
                new VertexPositionColor(new Vector3(100,-50,-100), Color.Gray),
            },0, 2);
            base.Draw(gameTime);
        }
    }
}
