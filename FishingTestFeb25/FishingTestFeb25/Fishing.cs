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

using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;


namespace FishingTestFeb25
{
    public enum FishingState
    {
        None, //fishing line is not cast
        Casting, //fishing line is thrown out 
        Waiting, //fishing line is dangling using rope physics
        Reeling, //fish is hooked
        Caught //fish is within range and caught
    }

    public class Fishing
    {
        public Game1 game;

        //these can be upgraded by player
        public int reelSpeed = 100;
        public int lineStrength = 100;

        public Rope rope;

        public FishingState state;

        public Fish caughtFish;
        public Curve pullCurve;

        public float pullStandard = 100;  //speed a normal fish "flees" changes based on fish strength
        public bool isPulling=true;
        public bool isCaught = false;
        public bool isBroken = false;

        
        public Vector2 position; //location of "pole"
        public Vector2 hookPosition;

        public float tension;
        public float maxTension = 100;


        public float MaxPull
        {
            get
            {
                if (caughtFish != null)
                {
                    return caughtFish.strength * pullStandard;
                }
                else
                {
                    return pullStandard;
                }
            }
        }

        public Fishing(Game1 game, Vector2 position)
        {
            this.game = game;
            this.state = FishingState.None;
            this.position = position;

            pullCurve = getPullCurve();

            rope = new Rope(game, position); 
            
            Reset();

        }

        private Curve getPullCurve()
        {
            Curve retval = new Curve();
            retval.Keys.Add(new CurveKey(0, 0));
            retval.Keys.Add(new CurveKey(.25f, .5f));
            retval.Keys.Add(new CurveKey(.5f, 1));
            retval.Keys.Add(new CurveKey(.8f, .5f));
            retval.Keys.Add(new CurveKey(1, 0));
            return retval;
        }

        private void Reset()
        {
            state = FishingState.Waiting;
            //position = new Vector2(100, 100);
            hookPosition = new Vector2(100, 300);
            rope = new Rope(game, position);
            
            isCaught = false;
            isPulling = false;
            isBroken = false;
            tension = 0;
            caughtFish = null;

        }

        public void Update(GameTime gameTime)
        {
            HandleInputFishingStats();
            switch (state)
            {
                case FishingState.None:
                    Reset();
                    break;
                case FishingState.Casting:

                    Reset();
                    break;
                case FishingState.Waiting:
                    UpdateWaiting(gameTime);
                    break;
                case FishingState.Reeling:
                    UpdateReeling2(gameTime);
                    break;
                case FishingState.Caught:
                    if (game.gameInput.IsNewKeyPress(Keys.Space))
                    {
                        Reset();
                    }
                    break;
            }
        }

        //used to adjust the fishing stats
        private void HandleInputFishingStats()
        {
            if (game.gameInput.IsKeyPress(Keys.Q))
            {
                lineStrength--;
                if (lineStrength < 0)
                    lineStrength = 0;
                       
            }
            if (game.gameInput.IsKeyPress(Keys.W))
            {
                lineStrength++;
               
            }
            if (game.gameInput.IsKeyPress(Keys.A))
            {
                reelSpeed--;
                if (reelSpeed < 0)
                    reelSpeed = 0;
            }
            if (game.gameInput.IsKeyPress(Keys.S))
            {
                reelSpeed++;
            }

            maxTension = lineStrength;
        }

        public void UpdateCasting(GameTime gameTime)
        {
            //set the hook position based on player control
        }

        //hook dangles on line
        public void UpdateWaiting(GameTime gameTime)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //handle input
            position.X += game.gameInput.HMovement * 500 * dt;
            hookPosition = rope.B;
            rope.Update(gameTime, position);

            //hookPosition.Y += game.gameInput.VMovement * 500 * dt;

            //move hook
            //hookPosition.X = position.X;


            //check if any fish collides with hook
            for (int i = 0; i < game.fishList.Count; i++)
            {
                if (game.fishList[i].BoundingCircle.Contains(hookPosition))
                {
                    
                    caughtFish = game.fishList[i];
                    game.DespawnFish(caughtFish);
                    state = FishingState.Reeling;
                    isPulling = true;
                    caughtFish.impulseTime = (float)game.r.NextDouble() * 2 + 1;
                    caughtFish.impulseTimer = TimeSpan.FromSeconds(caughtFish.impulseTime);
                    caughtFish.Velocity = getNewFishVector();
                     
                   
                }
            }
        }

        //fish is on the line and fighting
        //abstrac the fish behavior the fish object?
        public void UpdateReeling(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (tension > maxTension)
            {
                state = FishingState.None;
                game.SpawnFish(caughtFish.Position);
                caughtFish = null;

            }
            else
            {
                caughtFish.impulseTimer -= gameTime.ElapsedGameTime;
                if (caughtFish.impulseTimer < TimeSpan.Zero)
                {
                    caughtFish.impulseTimer = TimeSpan.FromSeconds((float)game.r.NextDouble() + 1);
                }

            }
        }

        public void UpdateReeling2(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            rope.Update(gameTime, position, caughtFish.Position);

            if (isCaught)
            {

                this.state = FishingState.Caught;
                
            }
            else if (isBroken)
            {
                this.state = FishingState.None;
                rope = new Rope(game, position);
                game.SpawnFish(caughtFish.Position);
               
            }
            else
            {

                if (Vector2.Distance(position,caughtFish.Position) < 50)
                {
                    game.caughtFish++;
                    this.state = FishingState.Caught;
                    return;
                }

                
                caughtFish.impulseTimer -= gameTime.ElapsedGameTime;
                if (caughtFish.impulseTimer < TimeSpan.Zero)
                {
                   
                    caughtFish.impulseTime = (float)game.r.NextDouble() * 2 + 1;
                    caughtFish.impulseTimer = TimeSpan.FromSeconds(caughtFish.impulseTime);
                    isPulling = !isPulling;

                    if (isPulling)
                    {
                        caughtFish.Velocity = getNewFishVector(); //fish slightly changes direction
                    }
                }

                HandleInput(gameTime);

                if (isPulling)
                {
                    if (tension > maxTension)
                    {
                        isBroken = true;
                        
                    }
                    else
                    {

                        float impulseRatio = (caughtFish.impulseTime - (float)caughtFish.impulseTimer.TotalSeconds) / caughtFish.impulseTime;
                        float pullCurveAmt = pullCurve.Evaluate(impulseRatio);

                        float dist = Vector2.Distance(position, caughtFish.Position);

                        float pullVectorAmt = MathHelper.Lerp(pullCurveAmt, pullCurveAmt/2, (tension / maxTension)) * MaxPull;
                        pullVectorAmt = MathHelper.Lerp(pullVectorAmt, 0, (dist / 1000));
                      
                        caughtFish.Position += caughtFish.Velocity * pullVectorAmt * dt;

                        //float tensionDif = MathHelper.Lerp(0, pullCurveAmt * caughtFish.strength, .5f);

                        tension += pullCurveAmt * caughtFish.strength * 5 * dt; ///arbitrary

                        //float tensionDif = MathHelper.Lerp(0, pullCurveAmt, .5f); //needs to factor in fish strength
                        //tension += tensionDif;
                    }

                }
                else
                {
                    //wait - bounce back B
                    //reset tension?  Or just drain it?
                    //this.B = startingB;

                    tension -= 100 * dt; //too fast?
                }

                if (tension < 0)
                {
                    tension = 0;
                }
            }
        }

        private void HandleInput(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 dif = caughtFish.Position - position;
            dif.Normalize();

            if (game.gameInput.HMovement < 0) //decrease line, increase tension
            {
                caughtFish.Position -= dif * reelSpeed * dt;
                tension += 50 * dt;

            }
            else if (game.gameInput.HMovement > 0)
            {
                caughtFish.Position += caughtFish.Velocity * reelSpeed * dt;
                tension -= 50 * dt;
            }
        }


        //gets an impulse for the fish to fight given the current tension
        private Vector2 getNewFishVector()
        {
            Vector2 newPos = new Vector2(caughtFish.Position.X + game.r.Next(-200, 200), caughtFish.Position.Y + game.r.Next(0, 100));

            Vector2 newDir = newPos - position;
            if(newDir.X != 0 && newDir.Y != 0)
            {
                newDir.Normalize();
            }

            return newDir;
           
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(game.FishingBoatTexture, position, Color.White);

            spriteBatch.DrawString(game.font1, state.ToString(), new Vector2(500, 600), Color.White);
         
            spriteBatch.DrawString(game.font1, "Q / W Line Strength: " + lineStrength.ToString(), new Vector2(100, 50), Color.White);
            spriteBatch.DrawString(game.font1, "A / S Reel Speed: " + reelSpeed.ToString(), new Vector2(100, 75), Color.White);
            spriteBatch.DrawString(game.font1, "Space: Cast line", new Vector2(100, 100), Color.White);
            spriteBatch.DrawString(game.font1, "Up: Line In / Down: Line Out", new Vector2(100, 125), Color.White);
            spriteBatch.DrawString(game.font1, "Left: Reel In / Right: Reel Out", new Vector2(100, 150), Color.White);

            if (state == FishingState.Reeling)
            {
                 float dist = Vector2.Distance(position, caughtFish.Position);

                
                 if (tension > 10 || dist < 300)
                 {
                     DrawPrimitives.drawLine(position, caughtFish.Position, 3, game.whitePixel, Color.Black, spriteBatch);
                 }
                 else
                 {
                     rope.Draw(gameTime, spriteBatch);
                 }
               
                float angle = (float)Math.Atan2((double)caughtFish.Velocity.Y, (double)caughtFish.Velocity.X);
                Vector2 fishVibratePos = new Vector2(caughtFish.Position.X + game.r.Next(-5, 5), caughtFish.Position.Y + game.r.Next(-5, 5));
                SpriteEffects flip = SpriteEffects.None;
                if (angle > MathHelper.PiOver2)
                {
                    flip = SpriteEffects.FlipVertically;
                }
                spriteBatch.Draw(caughtFish.texture, fishVibratePos, null, Color.Pink, angle, new Vector2(0, 0), caughtFish.strength / 5 + 1, flip, 0f);

                //caughtFish.Draw(gameTime, spriteBatch);

                Rectangle tensionRec = new Rectangle(100, 600, 300, 50);
                DrawPrimitives.DrawHealthBar(spriteBatch, game.whitePixel, tensionRec, Color.Red, true, true, (int)tension, (int)maxTension);
                spriteBatch.DrawString(game.font1, tension + " / " + maxTension, new Vector2(200, 600), Color.White);
               
                spriteBatch.DrawString(game.font1, "Line Distance: " + dist.ToString(), new Vector2(500, 650), Color.White);

            }

            if (state == FishingState.Waiting)
            {
                rope.Draw(gameTime, spriteBatch);
                //DrawPrimitives.drawLine(position, hookPosition, 2, game.whitePixel, Color.Pink, spriteBatch);
            }

            if (state == FishingState.Caught)
            {
               // spriteBatch.DrawString(game.font1, "Caught!", new Vector2(500, 500), Color.Black);
            }

        }
        
    

    }
}
