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
    public class Fish
    {
        public Game1 game;

        public Texture2D texture;
        public Vector2 Position;
        public Vector2 Velocity;

        private float radius;

        private int maxSpeed = 50;

        public TimeSpan impulseTimer;
        public float impulseTime = 5;

        public float strength; //0-10

        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, radius);
            }
        }

        public Fish(Game1 game, Texture2D texture, Vector2 position)
        {
            this.game = game;
            this.texture = texture;
            this.Position = position;
            this.Velocity = new Vector2(0, 0);
            this.radius = texture.Width / 2;

            this.strength = (float)game.r.NextDouble() * 20 + 1;


            
        }
        public void Update(GameTime gameTime)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            impulseTimer -= gameTime.ElapsedGameTime;
            if (impulseTimer < TimeSpan.Zero)
            {
                Velocity = getImpulse();
                impulseTimer = TimeSpan.FromSeconds( (float)game.r.NextDouble() * impulseTime);
            }

          //check bounds?


            if (Position.Y < 300)
            {
                Position.Y = 301;
                Velocity.Y *= -1;
            }


            if(Position.X < -10 || Position.X > 1400 || Position.Y < -100 || Position.Y > 900)
            {
                game.DespawnFish(this);
            }

            Position += Velocity * dt;

        }


        private Vector2 getImpulse()
        {
            return new Vector2(game.r.Next(-maxSpeed, maxSpeed), game.r.Next(-maxSpeed, maxSpeed));
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            SpriteEffects flip = SpriteEffects.FlipHorizontally;
            if(Velocity.X > 0)
            {
                flip = SpriteEffects.None;
            }

            DrawPrimitives.DrawCircle(BoundingCircle, game.whitePixel, Color.White, spriteBatch);


            spriteBatch.Draw(texture, Position, null, Color.White, 0f, new Vector2(0, 0), strength /5 + 1, flip, 0f);
            spriteBatch.DrawString(game.font1, strength.ToString(), Position, Color.White);


        }
            
    }
}
