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
using Microsoft.Kinect;


namespace XNA3DView
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont _spriteFont;
        Camera camera = new Camera();
        KinectSensor kinectSensor;

        Matrix cubeWorld;
        Matrix coneWorld;

        int width;
        int height;
        int depth = 1000;

        Model cubeModel;
        
        float xPos = 0;
        float yPos = 0;
        float zPos = 0;

        float xPosOffset = 660;
        float yPosOffset = -200;
        float zPosOffset = 0;


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
            cubeWorld = Matrix.Identity;
            //cubeWorld *= Matrix.CreateTranslation(100*cubeWorld.Backward);
            coneWorld = Matrix.Identity;
            kinectSensor = KinectSensor.KinectSensors[0];
            kinectSensor.SkeletonStream.Enable();
            
            kinectSensor.Start();

            width = GraphicsDevice.Viewport.TitleSafeArea.Width;
            height = GraphicsDevice.Viewport.TitleSafeArea.Height;
            base.Initialize();
        }

        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("SpriteFont1");

            cubeModel = Content.Load<Model>("Models\\p1_wedge");


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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            KeyboardState keyBoardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            var skeletFrame = kinectSensor.SkeletonStream.OpenNextFrame(10);

            if (skeletFrame == null) return;
            var skeletsData = new Skeleton[skeletFrame.SkeletonArrayLength];
            skeletFrame.CopySkeletonDataTo(skeletsData);

            //TODO linq nur 2 holen
            var trackedSkelets = skeletsData.ToList().Where(skelet => skelet.TrackingState == SkeletonTrackingState.Tracked);
            if (trackedSkelets.Count() < 1) return;

            var skeletFirst = trackedSkelets.First();

            Joint head = KinectAddons.JointExtensions.ScaleOwn(skeletFirst.Joints[JointType.Head], width*5, height*12, depth*3);
            //Joint head = skeletFirst.Joints[JointType.Head];
            //xPos = xPosOffset + head.Position.X * 100;
            //yPos = yPosOffset + head.Position.Y * 100;
            //zPos = zPosOffset + head.Position.Z * 100;

            xPos = xPosOffset + head.Position.X*-1;
            yPos = yPosOffset + head.Position.Y;
            //yPos = head.Position.Y;
            zPos = zPosOffset + head.Position.Z;



            if (keyBoardState.IsKeyDown(Keys.M))
            {
                yPos = yPos + 10;
            }
            if (keyBoardState.IsKeyDown(Keys.N))
            {
                yPos = yPos - 10;
            }
            //xPos = mouseState.X*10;
            //yPos = mouseState.Y*10;
            
                        
            //Rotate Cube along its Up Vector
            if (keyBoardState.IsKeyDown(Keys.X))
            {
                cubeWorld = Matrix.CreateFromAxisAngle(Vector3.Up, .02f) * cubeWorld;
            }
            if (keyBoardState.IsKeyDown(Keys.Z))
            {
                cubeWorld = Matrix.CreateFromAxisAngle(Vector3.Up, -.02f) * cubeWorld;
            }

            //Move Cube Forward, Back, Left, and Right
            if (keyBoardState.IsKeyDown(Keys.Up))
            {
                cubeWorld *= Matrix.CreateTranslation(cubeWorld.Forward);
            }
            if (keyBoardState.IsKeyDown(Keys.Down))
            {
                cubeWorld *= Matrix.CreateTranslation(cubeWorld.Backward);
            }
            if (keyBoardState.IsKeyDown(Keys.Left))
            {
                cubeWorld *= Matrix.CreateTranslation(-cubeWorld.Right);
            }
            if (keyBoardState.IsKeyDown(Keys.Right))
            {
                cubeWorld *= Matrix.CreateTranslation(cubeWorld.Right);
            }
            
            //camera.Update(new Vector3(head.Position.X,-1*head.Position.Y,head.Position.X));
            camera.Update(new Vector3(zPos, yPos, xPos));
            //camera.Update(new Vector3(skeletFirst.Joints[JointType.Head].Position.X, skeletFirst.Joints[JointType.Head].Position.Y, skeletFirst.Joints[JointType.Head].Position.Z));
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
            DrawModel(cubeModel, cubeWorld);
            DrawDebugStuff(spriteBatch);
                        
            spriteBatch.End();
        }

        private void DrawDebugStuff(SpriteBatch spriteBatch)
        {
            //return;
            int i = 1;
            var msg = String.Format("camera X:{0}/Y:{1}/Z:{2}", zPos,yPos,xPos);
            var msg2 = String.Format("real coord X:{0}/Y:{1}/Z:{2}", xPos, yPos, zPos);
            var measureString = _spriteFont.MeasureString(msg);
            var measureString2 = _spriteFont.MeasureString(msg2);

            measureString.Y = height - measureString.Y * i++;
            measureString.X = width / 2 - measureString.X / 2;
            measureString2.Y = height - measureString2.Y * i++;
            measureString2.X = width / 2 - measureString2.X / 2;

            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.White);
            spriteBatch.DrawString(_spriteFont, msg2, measureString2, Color.White);
        }

        private void DrawModel(Model model, Matrix worldMatrix)
        {
        Matrix[] modelTransforms = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(modelTransforms);

        foreach (ModelMesh mesh in model.Meshes)
        {
        foreach (BasicEffect effect in mesh.Effects)
        {
        effect.EnableDefaultLighting();
        effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
        effect.View = camera.viewMatrix;
        effect.Projection = camera.projectionMatrix;
        }
        mesh.Draw();
        }
        }
    }
}
