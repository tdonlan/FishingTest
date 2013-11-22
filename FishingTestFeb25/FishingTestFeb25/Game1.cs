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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        public GameInput gameInput;

        public Random r;

        public Texture2D fishTexture;
        public Texture2D whitePixel;
        public Texture2D FishingBoatTexture;

        public SpriteFont font1;

        public List<Fish> fishList = new List<Fish>();
        public Fishing fishing;

        public Rope rope;

        public LinePull linePull;

        public TimeSpan SpawnTimer;
        public float SpawnTime = 5f;

        public int caughtFish = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";

            r = new Random();

            gameInput = new GameInput();

            //linePull = new LinePull(this, new Vector2(100, 100), new Vector2(100, 300));
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

            fishTexture = Content.Load<Texture2D>("SmallFish");
            whitePixel = Content.Load<Texture2D>("WhitePixel");
            FishingBoatTexture = Content.Load<Texture2D>("FishingBoat");
            font1 = Content.Load<SpriteFont>("font1");

            PopulateLevel();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void PopulateLevel()
        {

            fishing = new Fishing(this, new Vector2(100, 50));


            for (int i = 0; i < r.Next(10, 20); i++)
            {
                SpawnFish(new Vector2(r.Next(0, 1000), r.Next(300, 500)));
            }

            //srope = new Rope(this, new Vector2(300, 100));
        }

        public void SpawnFish(Vector2 position)
        {
            fishList.Add(new Fish(this, fishTexture, position));
        }

        public void DespawnFish(Fish f)
        {
            fishList.Remove(f);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            SpawnTimer -= gameTime.ElapsedGameTime;
            if (SpawnTimer < TimeSpan.Zero)
            {
                SpawnFish(new Vector2(r.Next(100, 1000), r.Next(200, 500)));
                SpawnTimer = TimeSpan.FromSeconds((float)r.NextDouble() * SpawnTime + SpawnTime);
            }

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            gameInput.getInput(gameTime);

            // TODO: Add your update logic here
            for(int i=fishList.Count-1;i>=0;i--)
            {
                fishList[i].Update(gameTime);
            }

            fishing.Update(gameTime);
            //linePull.Update(gameTime);

            //rope.Update(gameTime, fishing.position + new Vector2(200, 0));

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

            Rectangle seaRec = new Rectangle(0,135,1280,720);
            DrawPrimitives.DrawRectangle(seaRec, whitePixel, Color.DarkBlue, spriteBatch, true, 1);

            // TODO: Add your drawing code here
            foreach (Fish f in fishList)
            {
                f.Draw(gameTime, spriteBatch);
            }

            fishing.Draw(gameTime, spriteBatch);
           // linePull.Draw(gameTime, spriteBatch);

            spriteBatch.DrawString(font1, " Caught: " + caughtFish.ToString(), new Vector2(1000, 600), Color.White);

            //rope.Draw(gameTime, spriteBatch);

            spriteBatch.End();

         
            

            base.Draw(gameTime);
        }
    }
}
