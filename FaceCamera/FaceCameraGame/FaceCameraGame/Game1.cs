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


namespace FaceCameraGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont _spriteFont;

        Vector3 cameraPos;
        int screenWidth;
        int screenHeight;

        FaceCamera.FaceCameraObject faceCamera = new FaceCamera.FaceCameraObject();
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
            // TODO: Add your initialization logic here

            base.Initialize();
            faceCamera.initialize();
            screenWidth = GraphicsDevice.Viewport.TitleSafeArea.Width;
            screenHeight = GraphicsDevice.Viewport.TitleSafeArea.Height;

            //start kinect here
            faceCamera.startKinect();
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

            // TODO: Add your update logic here
            
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
            DrawDebugStuff(spriteBatch);
            spriteBatch.End();
            
        }

        private void DrawDebugStuff(SpriteBatch spriteBatch)
        {
            //return;
            int i = 1;
            var msg = String.Format("camera X:{0}/Y:{1}/Z:{2}", cameraPos.X, cameraPos.Y, cameraPos.Z);
            var msg2 = String.Format("not in use");
            var measureString = _spriteFont.MeasureString(msg);
            var measureString2 = _spriteFont.MeasureString(msg2);

            measureString.Y = screenHeight - measureString.Y * i++;
            measureString.X = screenWidth / 2 - measureString.X / 2;
            measureString2.Y = screenHeight - measureString2.Y * i++;
            measureString2.X = screenWidth / 2 - measureString2.X / 2;

            spriteBatch.DrawString(_spriteFont, msg, measureString, Color.White);
            spriteBatch.DrawString(_spriteFont, msg2, measureString2, Color.White);
        }
    }
}
