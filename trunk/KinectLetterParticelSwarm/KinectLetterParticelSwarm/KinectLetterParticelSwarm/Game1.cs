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
using System.Diagnostics;

namespace KinectLetterParticelSwarm
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //room size
	    double width;
	    double height;
	    double depth;

	    //objects in room
	    FixedObjects[] fixedObjects;
        Attractor[] attractors;
        
	    
	    //Wörter
	    Word[] words;

        //Letters (come from words)
        private Letter[] letters;

        private float optAttractorDistance = 0.5f;
        private float dampingFactor = 1.0f;
        private float optAttractorPlayerDistance;
        private Vector3 player = new Vector3(5, 5, 10);

        
        float _fps = 1f;
        float FPS
        {
            get { return _fps; }
            set
            {
                _fps = value;
                TargetElapsedTime = TimeSpan.FromSeconds(1f / FPS);
                //Debug.WriteLine(_fps);
            }
        }
        private BasicEffect basicEffect;
        private int LETTER_SIZE = 500;
        private int ATTRACTOR_SIZE = 10;
        private float timeStep=0.000001f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            FPS = 60f;
            this.initLetterSwarm();
        }

        Vector3 CamPos { get; set; }
        Vector3 LookAt { get; set; }

	    //Kinect Kamera initialisieren

	    //Raumdefinition einlesen
	    void readRoom()
	    {
		    //read and place fixed objects
	    }

	    void initLetterSwarm()
	    {
            this.letters = new Letter[LETTER_SIZE];
            for (int i = 0; i < LETTER_SIZE; i++)
            {
                letters[i] = new Letter((i+1));
            }
            this.attractors = new Attractor[ATTRACTOR_SIZE];
            for (int i = 0; i < ATTRACTOR_SIZE; i++)
            {
                attractors[i] = new Attractor((i+1)*2);
            }
	    }

	    //Callback bei fertigem SkeletonFrames
	    void skeletonCallback(SkeletonFrame skeletonFrame)
	    {
            Boolean tracked = false;
		    //hier fertiges und getracktes skeleton
		    if(skeletonFrame != null && tracked==true)
		    {
			
		    }
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
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            basicEffect = new BasicEffect(GraphicsDevice);
            CamPos = new Vector3(5, 5, 80);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            RotateCam(Keyboard.GetState().GetPressedKeys());
            MovePlayer(Keyboard.GetState().GetPressedKeys());

            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Add) || Keyboard.GetState().GetPressedKeys().Contains(Keys.OemPlus))
                dampingFactor += 0.01f;
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Subtract) || Keyboard.GetState().GetPressedKeys().Contains(Keys.OemMinus))
                dampingFactor -= 0.01f;
            if (dampingFactor >= 1.0f)
            {
                dampingFactor = 1.0f;
            }
            Debug.WriteLine("dampingFactor " + dampingFactor);
            nextStep(timeStep);    
            base.Update(gameTime);
        }

              
        private void RotateCam(Keys[] keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.D1:
                        CamPos += new Vector3(2, 0, 0);
                        continue;
                    case Keys.D2:
                        CamPos += new Vector3(0, 2, 0);
                        continue;
                    case Keys.D3:
                        CamPos += new Vector3(0, 0, 2);
                        continue;
                    case Keys.D4:
                        CamPos -= new Vector3(2, 0, 0);
                        continue;
                    case Keys.D5:
                        CamPos -= new Vector3(0, 2, 0);
                        continue;
                    case Keys.D6:
                        CamPos -= new Vector3(0, 0, 2);
                        continue;
                    case Keys.D7:
                        CamPos = new Vector3(5, 5, 80);
                        continue;
                }
            }
            basicEffect.View = Matrix.CreateLookAt(CamPos, LookAt, Vector3.Up);
        }

        private void MovePlayer(Keys[] keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.W:
                        player += new Vector3(0.5f, 0, 0);
                        continue;
                    case Keys.A:
                        player += new Vector3(0, 0.5f, 0);
                        continue;
                    case Keys.S:
                        player -= new Vector3(0, 0, 0.5f);
                        continue;
                    case Keys.D:
                        player -= new Vector3(0, 0.5f, 0);
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

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, new[] { 
                    new VertexPositionColor(player, Color.Black), 
                    new VertexPositionColor(Vector3.Add(player,new Vector3(0,1,0)), Color.Green) 
                }, 0, 1);

            for (int i = 0; i < letters.Length;i++)
            {
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, new[] { 
                    new VertexPositionColor(letters[i].posNew, Color.Black), 
                    new VertexPositionColor(player, Color.Black) 
                }, 0, 1);
                //Debug.WriteLine("i["+i+"]:" + letters[i].posOld + " // " + letters[i].posNew);
            }

            for (int i = 0; i < attractors.Length; i++)
            {
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, new[] { 
                    new VertexPositionColor(attractors[i].posNew, Color.Red), 
                    new VertexPositionColor(player, Color.Red) 
                }, 0, 1);
                //Debug.WriteLine("i[" + i + "]:" + attractors[i].posOld + " // " + attractors[i].posNew);
            }

            //Debug.WriteLine("CamPos" + CamPos);
            //Debug.WriteLine("player" + player);
            base.Draw(gameTime);
                        
        }


        private void nextStep(float timeStep)
        {
            float stepSize = 0.1f;
            //calculate movement of attractors
                //calculate acc for oldPos
            Vector3[] accOld = calcObjAcc(attractors, attractors, true, 1.0f);
                //calculate new pos
            calcNewPos(attractors, accOld, stepSize);
                //calculate forces for new pos
            Vector3[]  accNew = calcObjAcc(attractors, attractors, true, 1.0f);
                //calculate velNew for new Pos
            calcNewVel(attractors, accOld, accNew, stepSize);
            
            //calculate movement for Letters

            accOld = calcObjAcc(letters, attractors, true, 0.1f);
            //calculate new pos
            calcNewPos(letters, accOld, stepSize);
            //calculate forces for new pos
            accNew = calcObjAcc(letters, attractors, true, 0.1f);
            //calculate velNew for new Pos
            calcNewVel(letters, accOld,accNew, stepSize);

            //calculate movement for words


        }

        private void calcNewVel(Letter[] letters, Vector3[] accOld, Vector3[] accNew, float stepSize)
        {
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i].velNew = Vector3.Add(letters[i].velOld, Vector3.Multiply(Vector3.Add(accOld[i], accNew[i]), 0.5f * stepSize));
                letters[i].velNew = Vector3.Multiply(letters[i].velNew, dampingFactor);
                letters[i].velOld = new Vector3(letters[i].velNew.X, letters[i].velNew.Y, letters[i].velNew.Z);
            }
        }

        private void calcNewPos(Letter[] letters, Vector3[] acc, float stepSize)
        {
            for(int i=0;i<letters.Length;i++)
            {
                letters[i].posNew = Vector3.Add(Vector3.Add(letters[i].posOld, Vector3.Multiply(letters[i].velOld, (float)stepSize)), Vector3.Multiply(acc[i], 0.5f * (float)Math.Pow(stepSize, 2)));
                letters[i].posOld = new Vector3(letters[i].posNew.X, letters[i].posNew.Y, letters[i].posNew.Z);
            }
            
        }

        private Vector3[] calcObjAcc(Letter[] toCalc, Letter[] reference,Boolean regardPlayer,float optDistance)
        {
            Vector3[] objAcc = new Vector3[toCalc.Length];
            for (int i = 0; i < toCalc.Length; i++)
            {
                    Vector3 direction = new Vector3(0,0,0);
                    for (int j = 0; j < reference.Length; j++)
                    {
                        if (i != j)
                        {
                            if (Vector3.Distance(toCalc[i].posNew, reference[j].posNew) < optDistance)
                            {
                                //this attractor is repelled
                                direction = Vector3.Add(Vector3.Subtract(toCalc[i].posOld, reference[j].posOld), direction);
                            }
                            else
                            {
                                //this attractors is attracted
                                direction = Vector3.Add(Vector3.Subtract(reference[j].posOld, toCalc[i].posOld), direction);
                            }
                             
                        }
                    }

                    if (regardPlayer)
                    {
                       //object is repelled from player
                        if (Vector3.Distance(toCalc[i].posNew, player) < optAttractorPlayerDistance)
                        {
                            direction = Vector3.Add(Vector3.Subtract(toCalc[i].posOld, player), direction);
                        }
                        else //object is attracted by player
                        {
                            direction = Vector3.Add(Vector3.Subtract(player, toCalc[i].posOld), direction);
                        }
                    }
                    objAcc[i] = direction;

            }
            return objAcc;
        }

    }
}
