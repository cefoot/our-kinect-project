using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using KinectAddons;
using Microsoft.Xna.Framework.Media;

namespace DynamicFluidSimulation
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        static int size = 320;
        Vector2[] particlePos = new Vector2[size];
        Vector2[] particlePosOld = new Vector2[size];
        Vector2[] particleVel = new Vector2[size];
        //Vector2[] particleVelOld = new Vector2[size];
        IList<Tuple<int,int,float>> neighbours = new List<Tuple<int,int,float>>();
        //IList<Tuple<int, int, float>> springs = new List<Tuple<int, int, float>>();
        Dictionary<Tuple<int, int>, float> springs = new Dictionary<Tuple<int, int>, float>();

        float omega = 2f;
        float alpha = 0.3f;
        float gamma=1f;
        float beta=0.01f;
        
        float k_spring = 0.2f;
        float rho_zero = 10.0f;
        float k = 0.004f;
        float k_near = 0.004f;
        float t = 1f;
        float maxDistGlobal = 10;
        float maxSpeed = 50;
        float cooling = 0.8f;

        Vector2 _g = new Vector2(0.0f, -9.81f);
        Vector2 frameSize; 

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
            frameSize = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.Width,GraphicsDevice.Viewport.TitleSafeArea.Height);
            Random r = new Random();
            // initialize particles with random pos
            for (int i = 0; i < particlePos.Length;++i)
            {
                particlePos[i] = new Vector2(50.0f+(float)(r.NextDouble() * (frameSize.X-200)), 50.0f+(float)(r.NextDouble() * (frameSize.Y-200)));
                System.Diagnostics.Debug.Print(particlePos[i].ToString());

                particleVel[i] = new Vector2((float)r.NextDouble() - 0.5f, (float)r.NextDouble() - 0.5f);
            }
            
            

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
            

            //apply forces
            //...

            computeDistance(maxDistGlobal);
            //apply Vicosity
            applyViscosity(maxDistGlobal, gameTime);

            savePos();


            //update positions
            predictPos(gameTime);

            //adjustSprings
            adjustSprings(maxDistGlobal,gameTime);

            //applySpringDisplacements
            applySpringDisplacements(maxDistGlobal, gameTime);
            
            //doubleDensityRelaxation
            doubleDensityRelaxiation(maxDistGlobal, gameTime);

            //resolveCollisions
            //not yet

            //update velocities
            updateVelocities(gameTime);

            resetParticles();

            KeyboardState newState = Keyboard.GetState();
            if (newState.IsKeyDown(Keys.Space))
            {
                drop();
            }

            base.Update(gameTime);
        }

        public void drop()
        {
            Random r = new Random();
            float dropDiameter=100.0f;
            //get some particles in the screen middle and move them closer together, than release
            for (int i = 0; i < particlePos.Length; ++i)
            {
                if(Vector2.Distance(particlePos[i],new Vector2(frameSize.X/2,frameSize.Y/2))<dropDiameter)
                {
                    particlePos[i] = new Vector2(frameSize.X / 2 + ((float)r.NextDouble() * dropDiameter - dropDiameter / 2), frameSize.Y / 2+((float)r.NextDouble() * dropDiameter - dropDiameter / 2));
                }
            }
        }

        public void resetParticles()
        {
            for (int i = 0; i < particlePos.Length;++i )
            {
                if (particlePos[i].X > frameSize.X)
                {
                    particlePos[i].X = 0;
                }
                else if (particlePos[i].X < 0)
                {
                    particlePos[i].X = frameSize.X;
                }

                if (particlePos[i].Y > frameSize.Y)
                {
                    particlePos[i].Y = 0;
                }
                else if (particlePos[i].Y < 0)
                {
                    particlePos[i].Y = frameSize.Y;
                }

            }
        }

        private void updateVelocities(GameTime gameTime)
        {
            for(int i=0;i<particleVel.Length;++i)
            {
                //particleVel[i] = Vector2.Subtract(particlePos[i], particlePosOld[i]) / (float)gameTime.TotalGameTime.TotalSeconds;
                particleVel[i] = Vector2.Subtract(particlePos[i], particlePosOld[i]) / t;
                particleVel[i] = particleVel[i] * cooling;
                if (particleVel[i].Length() > maxSpeed)
                {
                    particleVel[i] = particleVel[i] /particleVel[i].Length();
                }
            }
            
        }

        private void savePos()
        {
            for (int i = 0; i < particlePos.Length; ++i)
            {
                particlePosOld[i] = particlePos[i];
            }
        }

        public void doubleDensityRelaxiation(float maxDist, GameTime gameTime)
        {
            float rho = 0.0f;
            float rho_near = 0.0f;
            Vector2 d_x = new Vector2(0, 0);
            List<Tuple<int, int, float>> neigh_i = new List<Tuple<int, int, float>>();
            for (int i = 0; i < particlePos.Length; ++i)
            {
                rho = 0.0f;
                rho_near = 0.0f;
                neigh_i.Clear();
                foreach (var neigh in neighbours)
                {
                    //is it my neighbor?
                    if (neigh.Item1 == i && 1.0f>(neigh.Item3 / maxDist))
                    {
                        neigh_i.Add(neigh);
                        rho += 1 - (float)Math.Pow(1.0-(neigh.Item3 / maxDist),2);
                        rho_near += 1 - (float)Math.Pow(1.0 - (neigh.Item3 / maxDist), 3);
                    }
                }
                float preasure = k * (rho - rho_zero);
                float preasure_near = k_near * rho_near;
                d_x = new Vector2(0, 0);

                foreach (var neigh in neigh_i)
                {
                    float q=neigh.Item3 / maxDist;
                    if (q < 1)
                    {
                        Vector2 r_unit = Vector2.Subtract(particlePos[i],particlePos[neigh.Item2]);
                        r_unit.Normalize();
                        //float scale = (float)Math.Pow(gameTime.TotalGameTime.TotalSeconds,2.0)*(preasure*(1.0f-q)+preasure_near*(1.0f-(float)Math.Pow(q,2.0)));
                        float scale = (float)Math.Pow(t, 2.0) * (preasure * (1.0f - q) + preasure_near * (1.0f - (float)Math.Pow(q, 2.0)));
                        Vector2 displacement = Vector2.Multiply(r_unit,scale);
                        particlePos[neigh.Item2] = Vector2.Add(particlePos[neigh.Item2], displacement/2.0f);
                        d_x = Vector2.Subtract(d_x, displacement / 2);
                        //System.Diagnostics.Debug.Print(d_x.ToString);
                    }
                }
                particlePos[i] = Vector2.Add(particlePos[i], d_x);

            }
        }

        private void predictPos(GameTime gameTime)
        {
            for (int i = 0; i < particlePos.Length; ++i)
            {
                //x_i = x_i + t*v_i
                //particlePos[i] = Vector2.Add(particlePos[i], Vector2.Multiply(particleVel[i], (float)gameTime.ElapsedGameTime.TotalSeconds));
                particlePos[i] = Vector2.Add(particlePos[i], particleVel[i]*t);

            }
        }

        private void applyViscosity(float maxDist,GameTime time)
        {
            foreach (Tuple<int, int,float> triple in neighbours)
            {
                float q = triple.Item3 / maxDist;
                if (q < 1)
                {
                    Vector2 distVector = Vector2.Subtract(particleVel[triple.Item1], particleVel[triple.Item2]);
                    Vector2 r_unit = new Vector2(distVector.X,distVector.Y);
                    r_unit.Normalize();
                    float u = Vector2.Dot(distVector, r_unit);
                    if (u > maxSpeed)
                    {
                        u = (u / Math.Abs(u));
                    }
                    if (u > 0)
                    {
                        //float scale = (float)(time.ElapsedGameTime.TotalSeconds)*(1.0f - q) * (omega * u + beta *(float)Math.Pow((double)u, 2.0));
                        float scale = t * (1.0f - q) * (omega * u + beta * (float)Math.Pow((double)u, 2.0));
                        Vector2 I = Vector2.Multiply(r_unit, scale);
                        //System.Diagnostics.Debug.Print(scale.ToString());
                        particleVel[triple.Item1] = Vector2.Subtract(particleVel[triple.Item1], I / 2f);
                        particleVel[triple.Item2] = Vector2.Add(particleVel[triple.Item2], I / 2f);


                    }

                }
            }
        }

        private void computeDistance(float maxDist)
        {
            neighbours.Clear();
            for (int i = 0; i < particlePos.Length; ++i)
            {
                for (int j = 0; j < particlePos.Length; ++j)
                {
                    float dist = Vector2.Distance(particlePos[i], particlePos[j]);
                    if (i!=j && dist<maxDist)
                    {
                        //particles are close
                        Tuple<int,int,float> neigh = new Tuple<int, int,float>(i, j,dist);
                        neighbours.Add(new Tuple<int, int,float>(i, j,dist));

                        //add spring
                        if(!springs.ContainsKey(new Tuple<int,int>(i,j)))
                        {
                            springs.Add(new Tuple<int,int>(i,j), dist);
                        }

                    }
                    else if (dist > maxDist)
                    {
                        springs.Remove(new Tuple<int, int>(i, j));
                    }

                }
            }
        }

        public void adjustSprings(float maxDist, GameTime gameTime)
        {
            foreach (var neigh in neighbours)
            {
                float dist = Vector2.Distance(particlePos[neigh.Item1], particlePos[neigh.Item2]);
                float q = dist/maxDist;
                if (q < 1.0f)
                {
                    Tuple<int,int> possibleSpring = new Tuple<int, int>(neigh.Item1, neigh.Item2);
                    if (!springs.ContainsKey(possibleSpring))
                    {
                        springs.Add(possibleSpring,maxDist);
                    }
                        //is deformation tolerable
                    float springValue;
                    bool springFound = springs.TryGetValue(possibleSpring, out springValue);
                    if (springFound && dist > maxDist + gamma * springValue)
                    {
                        //springValue = springValue + (float)gameTime.ElapsedGameTime.TotalSeconds * alpha * (dist - springRestLength - gamma * springValue);
                        springValue = springValue + t * alpha * (dist - maxDist - gamma * springValue);
                    }
                    else if (springFound && dist < maxDist - gamma * springValue)
                    {
                        //springValue = springValue - (float)gameTime.ElapsedGameTime.TotalSeconds * alpha * (dist - springRestLength - gamma * springValue);
                        springValue = springValue - t * alpha * (maxDist - gamma * springValue - dist);
                    }
                    if (springValue > 10.00000f)
                    {
                        //System.Diagnostics.Debug.Print(springValue.ToString());
                        springValue = springValue / Math.Abs(springValue);
                    }
                    if (springFound)
                    {
                        springs[possibleSpring] = springValue;
                        
                    }
                    
                }

            }

            List<Tuple<int, int>> keys = new List<Tuple<int, int>>();
            foreach(var spring in springs)
            {
                if (spring.Value > maxDist)
                {
                    keys.Add(spring.Key);

                }
            }
            foreach (var key in keys)
            {
                springs.Remove(key);
            }
        }

        public void applySpringDisplacements(float maxDist, GameTime gameTime)
        {
            foreach (var spring in springs)
            {
                float dist = Vector2.Distance(particlePos[spring.Key.Item1],particlePos[spring.Key.Item2]);
                //float scale = (float) Math.Pow(gameTime.TotalGameTime.TotalSeconds, 2) * k_spring * (1.0f - spring.Value / maxDist) * (spring.Value - dist);
                //System.Diagnostics.Debug.Print(spring.Value.ToString());
                float scale = (float)Math.Pow(t,2) * k_spring * (1.0f - spring.Value / maxDist) * (spring.Value - dist);
                //System.Diagnostics.Debug.Print(scale.ToString());
                if (scale > 1000000f)
                {
                    scale = scale / Math.Abs(scale);
                }
                Vector2 displacement = Vector2.Subtract(particlePos[spring.Key.Item1], particlePos[spring.Key.Item2]);
                displacement.Normalize();
                displacement = displacement * scale;
                particlePos[spring.Key.Item1] = Vector2.Subtract(particlePos[spring.Key.Item1],displacement/2.0f);
                particlePos[spring.Key.Item2] = Vector2.Add(particlePos[spring.Key.Item2], displacement / 2.0f);
            }
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            var primitiveLine = new PrimitiveLine(GraphicsDevice);
            
            for(int i=0;i<particlePos.Length;++i)
            {
                primitiveLine.ClearVectors();
                primitiveLine.Colour = Color.Red;
                primitiveLine.Position=particlePos[i];
                primitiveLine.CreateCircle(5, 5);
                primitiveLine.Render(spriteBatch);
                //System.Diagnostics.Debug.Print(particlePos[i].ToString());

            }

            spriteBatch.End();
            //base.Draw(gameTime);
        }
    }
}
