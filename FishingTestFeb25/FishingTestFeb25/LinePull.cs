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

namespace FishingTestFeb25
{
    //Fishing mechanics on a single dimension
    public class LinePull
    {
        public Game1 game;

        public Vector2 A;
        public Vector2 B;

        public Vector2 startingB;

        public Vector2 pullVector;
        public Curve pullCurve;
        public float maxPull = 200;  //maximum "speed" of the pull


        public TimeSpan ImpulseTimer;
        public float ImpulseTime;
        public bool isPulling;

        public float Tension = 0;
        public float maxTension = 200;

        public bool isCaught = false;
        public bool isBroken = false;

        public LinePull(Game1 game, Vector2 A, Vector2 B)
        {
            this.game = game;
            this.A = A;
            this.B = B;

            this.startingB = B;
            this.ImpulseTime = (float)game.r.NextDouble() * 2;
            this.ImpulseTimer = TimeSpan.FromSeconds(ImpulseTime);
            this.isPulling = false;

            this.pullCurve =  getPullCurve();

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

        private Curve getPullCurve2()
        {
            Curve retval = new Curve();
            retval.Keys.Add(new CurveKey(0, 0));
            retval.Keys.Add(new CurveKey(.5f, 1f));
            retval.Keys.Add(new CurveKey(0f, 0));
    
            return retval;
        }

        private Curve getPullCurve3()
        {
            Curve retval = new Curve();
            retval.Keys.Add(new CurveKey(0, 0));
            retval.Keys.Add(new CurveKey(.1f, .9f));
            retval.Keys.Add(new CurveKey(.5f, 1));
            retval.Keys.Add(new CurveKey(.9f, .9f));
            retval.Keys.Add(new CurveKey(1, 0));
            return retval;

        }

        private void Reset()
        {
            isCaught = false;
            isPulling = false;
            isBroken = false;
            Tension = 0;
            A = new Vector2(100, 100);
            B = new Vector2(100, 300);

        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isCaught)
            {
                if (game.gameInput.IsNewKeyPress(Keys.Space))
                {
                    Reset();
                }
            }
            else if (isBroken)
            {
                if (game.gameInput.IsNewKeyPress(Keys.Space))
                {
                    Reset();
                }
            }
            else
            {

                if (Math.Abs(A.Y - B.Y) < 50)
                {
                    isCaught = true;
                    return;
                }

                this.ImpulseTimer -= gameTime.ElapsedGameTime;
                if (ImpulseTimer < TimeSpan.Zero)
                {
                    ImpulseTime = (float)game.r.NextDouble() * 2;
                    ImpulseTimer = TimeSpan.FromSeconds(ImpulseTime);
                    isPulling = !isPulling;
                }

                HandleInput(gameTime);

                if (isPulling)
                {
                    if (Tension > maxTension)
                    {
                        isBroken = true;
                    }
                    else
                    {

                        float impulseRatio = (ImpulseTime - (float)ImpulseTimer.TotalSeconds) / ImpulseTime;
                        float pullVectorAmt = pullCurve.Evaluate(impulseRatio);

                        float dist = Vector2.Distance(A,B);
                        float pullDistAmount = MathHelper.Lerp(maxPull, 0, dist / 1000); //if the fish is super far away, pull less
                        Vector2 pullVector = new Vector2(0, MathHelper.Lerp(pullVectorAmt, 0, (Tension / maxTension))) * pullDistAmount;

                        B += pullVector * dt;

                        float tensionDif = MathHelper.Lerp(0, pullVectorAmt * 100, .5f);
                        Tension += tensionDif * dt;
                    }

                }
                else
                {
                    //wait - bounce back B
                    //reset tension?  Or just drain it?
                    //this.B = startingB;
                   
                    this.Tension -= 100 * dt; //too fast?
                }

                if (Tension < 0)
                {
                    Tension = 0;
                }
            }

        }


        private void HandleInput(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float dist = B.Y - A.Y;
            if (game.gameInput.HMovement < 0) //decrease line, increase tension
            {
               
                B.Y -= 100 * dt;
                Tension += 50 * dt;

            }
            else if (game.gameInput.HMovement > 0)
            {
                B.Y += 100 * dt;
                Tension -= 50 * dt;
            }
        }


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawPrimitives.drawLine(A, B, 5, game.whitePixel, Color.Red, spriteBatch);

            Rectangle tensionRec = new Rectangle(200,200,300,50);
            DrawPrimitives.DrawHealthBar(spriteBatch, game.whitePixel, tensionRec, Color.Red, true, true, (int)Tension, (int)maxTension);

            float dist = Vector2.Distance(A, B);
            spriteBatch.DrawString(game.font1, "Line Distance: "+dist.ToString(), new Vector2(500, 600), Color.Black);

            if (isCaught)
            {
                spriteBatch.DrawString(game.font1, "Caught!", new Vector2(500, 500), Color.Black);
            }
            if (isBroken)
            {
                spriteBatch.DrawString(game.font1, "Broken!", new Vector2(500, 500), Color.Black);
            }
        }

    }
}
