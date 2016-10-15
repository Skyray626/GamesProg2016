﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;                                       

namespace PRedesign.src.Objects.Testing
{
    class ExitPortal : BasicModel
    {
        Player player;

        bool isUnlocked;
        SphereCollider collider;
        //bool keyRelocated = false;
        //Key needs  to have a door related with it
        AudioEmitterComponent audioEmitter;

        // animation variables
        private bool opening = false;
        private bool isSplit;
        private float deltaTime;
        private float rotationalSpeed = 1f; //speed of rotation
        private float activateDistance = 35f; //distance between player and object to activate
        private float outerMaxDistance = 25f; //how far the outer parts can move
        private float currentDistance; //how far the outers are currently
        private float outerMovementSpeed = 2f; //how quickly the outer parts move

        // hover animation variables
        private float hoverHeight;
        private float originalYPosition;
        private float hoverSpeed = 0.8f;

        public ExitPortal(Vector3 startPosition, Model model, BasicCamera camera, Player player) : base(startPosition, camera, model)
        {
            this.player = player;
            isUnlocked = false; //false
            orientation = 0f;
            scale = 0.03f;
            scaleMatrix = Matrix.CreateScale(scale);

            //Collision code. Will the door tag work correctly? Should we start this off being an obstacle and then make it a door tag?
            collider = new SphereCollider(this, ObjectTag.exit, 3f);
            collider.DrawColour = Color.Yellow;

            CollisionManager.ForceTreeConstruction();

            audioEmitter = new AudioEmitterComponent(this);
            //audioEmitter.addSoundEffect("pickup", game.Content.Load<SoundEffect>(@"Sounds/key"));

            //Set up animation variables
            isSplit = false;
            //scaleMatrix = Matrix.CreateScale(0.05f);
            currentDistance = 0f;
            hoverHeight = 0f;
            originalYPosition = startPosition.Y;

            hasLighting = true;
        }

        public override void Update(GameTime gameTime)
        {
            //Doesn't do anything out of ordinary if locked
            /*Exit portal should just stay open.
            if (Vector3.Distance(this.position, player.Position) < activateDistance)
                opening = true;
            else
                opening = false;*/

            List<Collider> currentCollisions = collider.getCollisions();
            if (currentCollisions.Count() > 0)
            {
                foreach (Collider col in currentCollisions)
                {
                    if (col.Tag == ObjectTag.player)
                    {
                        //Game is over
                        LevelManager.ReloadLevel();
                        return;
                    }
                }
            }

            //Animate the unlocking sequence if needed
            //Animate opening if player is close
            

            base.Update(gameTime);
        }

        private void rotateKey(GameTime gameTime)
        {
            deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;

            orientation += rotationalSpeed * deltaTime;
            //resets to zero + overlap
            if (orientation > MathHelper.TwoPi)
            {
                float overlap = orientation - MathHelper.TwoPi;
                orientation = 0f + overlap;
            }
            //Otherwise rotate the outside counterclockwise
            if (isSplit)
            {
                rotationMatrix = Matrix.CreateRotationY(-orientation);
                model.Bones["top_geo"].Transform = Matrix.CreateRotationY(orientation * 2);
                model.Bones["bot_geo"].Transform = Matrix.CreateRotationY(orientation * 2);
            }
            else
            {
                rotationMatrix = Matrix.CreateRotationY(orientation);
                model.Bones["top_geo"].Transform = Matrix.CreateRotationY(0f);
                model.Bones["bot_geo"].Transform = Matrix.CreateRotationY(0f);
            }
        }

        private void unlockAnimation()
        {
            //Moves them outwards or inwards
            //Needs to accelerate and decelerate as they go past? or just move it out until it hits the limit
            if (isSplit && currentDistance <= outerMaxDistance)
            {
                currentDistance += outerMovementSpeed;
            }
            if (!isSplit && currentDistance > 0)
            {
                currentDistance -= outerMovementSpeed;
            }
            //currentDistance += (isSplit ? outerMovementSpeed : -outerMovementSpeed);
            model.Bones["top_geo"].Transform *= Matrix.CreateTranslation(0f, currentDistance, 0f);
            model.Bones["bot_geo"].Transform *= Matrix.CreateTranslation(0f, -currentDistance, 0f);

        }

        public override Matrix GetWorld()
        {
            translationMatrix = Matrix.CreateTranslation(position);
            return scaleMatrix * rotationMatrix * translationMatrix;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!isUnlocked)
                base.Draw(gameTime);
        }
    }
}
