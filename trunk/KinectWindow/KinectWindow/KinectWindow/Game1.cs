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
using FaceCamera;

namespace KinectWindow
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //this.graphics.PreferredBackBufferWidth = 480;
            //this.graphics.PreferredBackBufferHeight = 800;
            //graphics.IsFullScreen = true;
        }

        Quad backWall;
        Quad leftWall;
        Quad rightWall;
        Quad upWall;
        Quad downWall;
        Model brbTor;
        VertexDeclaration quadVertexDecl;
        Matrix View, Projection;

        Texture2D texture;
        BasicEffect quadEffect;
        FaceCameraObject FaceObj;
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //draufsicht 
            //new normalVec(0, 0, -1);
            //new upVec(0, 1, 0);
            backWall = new Quad(new Vector3(0, 0, 100), new Vector3(0, 0, -1), new Vector3(0, 1, 0), 64, 48);
            leftWall = new Quad(new Vector3(32, 0, 40), new Vector3(-1, 0, 0), new Vector3(0, 1, 0), 120, 48);
            rightWall = new Quad(new Vector3(-32, 0, 40), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 120, 48);
            downWall = new Quad(new Vector3(0, -24, 40), new Vector3(0, 1, 0), new Vector3(1, 0, 0), 120, 64);
            upWall = new Quad(new Vector3(0, 24, 40), new Vector3(0, -1, 0), new Vector3(-1, 0, 0), 120, 64);
            CameraPosition = new Vector3(0, 0, -50);
            CameraLookAt = new Vector3(0, 0, -20);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4.0f / 3.0f, 1, 500);

            FaceObj = new FaceCameraObject();
            FaceObj.Initialize();

            base.Initialize();
        }

        public Vector3 CameraPosition { get; set; }

        public Vector3 CameraLookAt { get; set; }

        private void UpdateView()
        {
            View = Matrix.CreateLookAt(CameraPosition, CameraLookAt, Vector3.Up);
            quadEffect.View = View;
        }

        protected override void LoadContent()
        {
            // TODO: Load any ResourceManagementMode.Automatic content
            texture = Content.Load<Texture2D>("Wall1");
            quadEffect = new BasicEffect(graphics.GraphicsDevice);
            quadEffect.EnableDefaultLighting();
            quadEffect.World = Matrix.Identity;
            quadEffect.Projection = Projection;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = texture;
            UpdateView();
            // TODO: Load any ResourceManagementMode.Manual content
            quadVertexDecl = new VertexDeclaration(VertexPositionNormalTexture.VertexDeclaration.GetVertexElements());
            FaceObj.StartKinect();

            brbTor =  Content.Load<Model>("ant");
        }

        protected override void Update(GameTime gameTime)
        {
            var newState = Keyboard.GetState();
            if (newState.IsKeyDown(Keys.Left))
            {
                CameraPosition += new Vector3(1, 0, 0);
            }
            if (newState.IsKeyDown(Keys.Right))
            {
                CameraPosition += new Vector3(-1, 0, 0);
            }
            if (newState.IsKeyDown(Keys.Up))
            {
                CameraPosition += new Vector3(0, 1, 0);
            }
            if (newState.IsKeyDown(Keys.Down))
            {
                CameraPosition += new Vector3(0, -1, 0);
            }
            if (newState.IsKeyDown(Keys.F))
            {
                graphics.ToggleFullScreen();
            }
            var newVec = FaceObj.GetNewCameraPosition(new Vector3(0, -20, -50), new Vector3(50, -50, 0), new Vector3(-50, 50, 0));
            if (newVec != new Vector3(0, -20, -50))
            {
                CameraPosition = newVec;
            }
            worldMatrix = Matrix.CreateWorld(new Vector3(0, -24, 40), Vector3.Left , Vector3.Up);
            worldMatrix *= Matrix.CreateScale(0.2f);
            UpdateView();
            base.Update(gameTime);
        }

        protected override void UnloadContent()
        {
            Content.Unload();
            base.UnloadContent();
        }

        Matrix worldMatrix;

        protected override void Draw(GameTime gameTime)
        {
            foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, backWall.Vertices, 0, 4, backWall.Indices, 0, 2);
                graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, leftWall.Vertices, 0, 4, leftWall.Indices, 0, 2);
                graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, rightWall.Vertices, 0, 4, rightWall.Indices, 0, 2);
                graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, upWall.Vertices, 0, 4, upWall.Indices, 0, 2);
                graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, downWall.Vertices, 0, 4, downWall.Indices, 0, 2);

            }
            brbTor.Draw(worldMatrix, View, Projection);
        }
    }
}
