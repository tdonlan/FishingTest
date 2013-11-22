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
    public class Rope
    {
        public Game1 game;

        public Vector2 A;
    

        public World world;

        Body AnchorBody;

        public List<Body> ChainLinkList = new List<Body>();
        //public List<RopeJoint> ChainJointList = new List<RopeJoint>();
        public List<RevoluteJoint> ChainJointList = new List<RevoluteJoint>();

        private int chainCount = 10;
        private int chainLength = 10;
        private float ropeLength = 1f;

        public Vector2 B
        {
            get
            {
                return ConvertUnits.ToDisplayUnits(ChainLinkList[ChainLinkList.Count - 1].Position);
            }
        }

        public Rope(Game1 game, Vector2 A)
        {
            this.game = game;
            this.A = A;
        

            this.world = new World(Vector2.UnitY );
            /*
            AnchorBody = BodyFactory.CreateCircle(world, 1, 1);
            AnchorBody.Position = ConvertUnits.ToSimUnits(A);
            AnchorBody.BodyType = BodyType.Static;
            */


            for (int i = 0; i < chainCount; i++)
            {
                ChainLinkList.Add(BodyFactory.CreateRectangle(world, 1, ConvertUnits.ToSimUnits(chainLength), .01f));
               
                ChainLinkList[i].BodyType = BodyType.Dynamic;
                ChainLinkList[i].Position =  ConvertUnits.ToSimUnits(A);
            }

            //CreateRevoluteJoint(AnchorBody, ChainLinkList[0], new Vector2(0, 0), ConvertUnits.ToSimUnits(new Vector2(0, -chainLength)));

            for (int i = 0; i < chainCount - 1; i++)
            {
                CreateRevoluteJoint(ChainLinkList[i], ChainLinkList[i + 1], ConvertUnits.ToSimUnits(new Vector2(0, chainLength)), ConvertUnits.ToSimUnits(new Vector2(0, -chainLength)));
            }

            ChainLinkList[0].Position = ConvertUnits.ToSimUnits(A);
            ChainLinkList[0].BodyType = BodyType.Kinematic;

        }

        private void CreateRopeJoint(Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            RopeJoint ropeJoint = new RopeJoint(bodyA, bodyB, anchorA, anchorB);
            ropeJoint.CollideConnected = false;
            ropeJoint.MaxLength = ConvertUnits.ToSimUnits(ropeLength);
            //ChainJointList.Add(ropeJoint);
            world.AddJoint(ropeJoint);
        }

        private void CreateRevoluteJoint(Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB)
        {
            RevoluteJoint revJoint = new RevoluteJoint(bodyA, bodyB, anchorA, anchorB);

            revJoint.CollideConnected = false;
            revJoint.Breakpoint = 5000;
           
          
            ChainJointList.Add(revJoint);
            world.AddJoint(revJoint);
        }
       
        //used while waiting
        public void Update(GameTime gameTime, Vector2 A)
        {
            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
            ChainLinkList[0].Position = ConvertUnits.ToSimUnits(A);
            //AnchorBody.Position = ConvertUnits.ToSimUnits(A);

            HandleInput(gameTime);
        }

        //used when fish is on the hook
        public void Update(GameTime gameTime, Vector2 A, Vector2 B)
        {
            FixRopeLength(A, B);

            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
            ChainLinkList[0].Position = ConvertUnits.ToSimUnits(A);
            ChainLinkList[chainCount-1].Position = ConvertUnits.ToSimUnits(B);

           
        }

        private void FixRopeLength(Vector2 A, Vector2 B)
        {
            float lineLen = chainCount * 100;
            float dist = Vector2.Distance(A,B);
            if (dist > 300)
            {

                if (lineLen + chainLength*2 > dist)
                {
                    RemoveRope();
                }
                else if (lineLen - chainLength*2 < dist)
                {
                    AddRope();
                }
            }

            

        }

        private void HandleInput(GameTime gameTime)
        {
            if (game.gameInput.IsNewKeyPress(Keys.Down))
            {
                if (chainCount < 30)
                {
                    AddRope();
                }
            }
            if (game.gameInput.IsNewKeyPress(Keys.Up))
            {
                if (chainCount > 2)
                {
                    RemoveRope();
                }
            }
                

        }

        //adds a single length of rope to end of rope
        private void AddRope()
        {

            ChainLinkList.Add(BodyFactory.CreateRectangle(world, 1, ConvertUnits.ToSimUnits(chainLength), .1f));
            ChainLinkList[chainCount].BodyType = BodyType.Dynamic;
            ChainLinkList[chainCount].Position = ChainLinkList[chainCount - 1].Position;
            CreateRevoluteJoint(ChainLinkList[chainCount - 1], ChainLinkList[chainCount], ConvertUnits.ToSimUnits(new Vector2(0, chainLength)), ConvertUnits.ToSimUnits(new Vector2(0, -chainLength)));
            chainCount++;
        }

        //removes a single length of rope from the end
        private void RemoveRope()
        {

            RevoluteJoint joint = ChainJointList[ChainJointList.Count - 1];
            world.RemoveJoint(joint);
            ChainJointList.Remove(joint);

            Body b = ChainLinkList[chainCount - 1];
            world.RemoveBody(b);
            ChainLinkList.Remove(b);
            chainCount--;

        }

        

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            for (int i = 0; i < ChainLinkList.Count-1; i++)
            {
                Vector2 A = ConvertUnits.ToDisplayUnits(ChainLinkList[i].Position);
                Vector2 B = ConvertUnits.ToDisplayUnits(ChainLinkList[i + 1].Position);
                DrawPrimitives.drawLine(A, B, 3, game.whitePixel, Color.Black, spriteBatch);
            }

            /*
            foreach(RevoluteJoint joint in ChainJointList)
            {
                Vector2 A = ConvertUnits.ToDisplayUnits(joint.WorldAnchorA);
                Vector2 B = ConvertUnits.ToDisplayUnits(joint.WorldAnchorB);
                DrawPrimitives.drawLine(A,B,5,game.whitePixel,Color.Pink,spriteBatch);
            }

            //spriteBatch.Draw(game.whitePixel, ConvertUnits.ToDisplayUnits(AnchorBody.Position), null, Color.Red, 0f, new Vector2(0, 0), 10f, SpriteEffects.None, 0);
            foreach (Body b in ChainLinkList)
            {
                spriteBatch.Draw(game.whitePixel, ConvertUnits.ToDisplayUnits(b.Position),null, Color.Purple,0f,new Vector2(0,0),10f,SpriteEffects.None,0);
            }
             * */

        }

    }
}
