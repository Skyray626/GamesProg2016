﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace PRedesign
{
    class Player : GameObject
    {
        #region Fields
        BasicCamera camera;

        Vector3 lookDirection;
        Vector3 headHeightOffset = new Vector3(0, 5, 0);
        MouseState prevMouseState;
        float pitchRotationRate = MathHelper.PiOver4 / 150;
        float currentPitch = 0;// = MathHelper.PiOver2;
        float maxPitch = MathHelper.PiOver2 * (19 / 20f);

        Game game;

        //Movement varibles
        Vector3 movementDirection;
        Vector3 acceleration;
        float accelerationRate = 200f;
        float decelerationRate = 300f;
        float maxVelocity = 50f;//20f;
        float minVelocity = 5f;
        float deltaTime = 0;


        bool jumped = false;
        bool grounded = true;
        float jumpVelocity = 50f;
        float fallRate = 200f; // Is gravity


        /// <summary>
        /// For lab task
        /// </summary>
        Tank tank;
        public Tank Tank {
            get { return tank; }
            set { tank = value; }
        }
        #endregion

        #region Properties
        public BasicCamera Camera {
            get { return camera; }
            set { camera = value; }
        }

        public Vector3 HeadHeightOffset {
            get { return headHeightOffset; }
            set { headHeightOffset = value; }
        }

        public Game Game {
            get { return game; }
            set { game = value; }
        }
        #endregion

        #region Initialization
        public Player(Vector3 startPosition) : base(startPosition)
        {
            game = ObjectManager.Game;
            camera = ObjectManager.Camera;

            camera.setPositionAndDirection(position + headHeightOffset, lookDirection);
            lookDirection = Vector3.Left;

            if (game != null)
                if (game.Window != null)
                    Mouse.SetPosition(game.Window.ClientBounds.Width / 2, game.Window.ClientBounds.Height / 2);
        }
        #endregion

        #region Update and Draw
        public override void Update(GameTime gameTime)
        {
            handleInput();
            handleMovement(gameTime);

            handleMouseSelection();
            camera.setPositionAndDirection(position + headHeightOffset, lookDirection);

            base.Update(gameTime);
        }

        private void handleInput() {
            //Get the states of the keyboard and mouse
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            //Resets the movement direction to zero, then checks which keys are pressed to create a new movement direction.
            if (lookDirection != Vector3.Zero)
                lookDirection.Normalize();
            
            //Sets the yaw rotation of the cameras' look direction
            lookDirection = Vector3.Transform(lookDirection, Matrix.CreateFromAxisAngle(Vector3.Up, (-MathHelper.PiOver4 / 150) * (mouseState.X - prevMouseState.X)));

            //Sets the pitch rotation of the cameras look direction, maxes out so that the player cant look directly up or down
            float pitchAngle = (pitchRotationRate) * (mouseState.Y - prevMouseState.Y);
            if (Math.Abs(currentPitch + pitchAngle) < maxPitch)
            {
                lookDirection = Vector3.Transform(lookDirection, Matrix.CreateFromAxisAngle(Vector3.Cross(Vector3.Up, lookDirection), pitchAngle));
                currentPitch += pitchAngle;
            }

            //Calculate players movement choices
            movementDirection = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.W))
                movementDirection += lookDirection;
            if (keyboardState.IsKeyDown(Keys.S))
                movementDirection -= lookDirection;
            if (keyboardState.IsKeyDown(Keys.A))
                movementDirection += Vector3.Cross(Vector3.Up, lookDirection);
            if (keyboardState.IsKeyDown(Keys.D))
                movementDirection -= Vector3.Cross(Vector3.Up, lookDirection);
            //Set jumped to true if the condition is correct
            jumped = (keyboardState.IsKeyDown(Keys.Space) && !jumped);


            //Resets the mouses position to the center of the screen, also resets the prevouse mouse state so the camera wont jump around
            if (game != null)
                if(game.Window != null)
                    Mouse.SetPosition(game.Window.ClientBounds.Width / 2, game.Window.ClientBounds.Height / 2);
            prevMouseState = Mouse.GetState();
        }

        //Without collision detection at the moment
        private void handleMovement(GameTime gameTime)
        {

            deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;

            if (movementDirection != Vector3.Zero)
                movementDirection.Normalize();

            //Acceleration and deceleration if the player is moving or not
            acceleration = movementDirection * accelerationRate;
            Vector2 horizontalVelocity = new Vector2(velocity.X, velocity.Z);

            if (acceleration == Vector3.Zero && horizontalVelocity != Vector2.Zero)
            {
                Vector2 horizontalDeceleration = Vector2.Normalize(horizontalVelocity) * -decelerationRate;
                acceleration = new Vector3(horizontalDeceleration.X, acceleration.Y, horizontalDeceleration.Y);
                if (horizontalVelocity.Length() < minVelocity)
                {
                    velocity = Vector3.Zero;
                }
            }
            //Remove y component to be calculated seperatly;
            acceleration.Y = 0;
            velocity += acceleration * deltaTime;

            if (horizontalVelocity.Length() > maxVelocity)
            {
                horizontalVelocity = Vector2.Normalize(horizontalVelocity) * (maxVelocity - 1);
                velocity = new Vector3(horizontalVelocity.X, velocity.Y, horizontalVelocity.Y);
            }

            acceleration = Vector3.Zero;
            if (!grounded)
                acceleration = Vector3.Down * fallRate;

            if (grounded && jumped)
            {
                velocity += Vector3.Up * jumpVelocity;
                grounded = jumped = false;
            }

            velocity += acceleration * deltaTime;

            if (position.Y < 0)
            {
                velocity.Y = 0;
                acceleration.Y = 0;
                grounded = true;
                position = new Vector3(position.X, 0, position.Z);
            }
            
            //Colision code goes here

            position += velocity * deltaTime;

        }
        #endregion


        #region Helper Methods
        private void handleMouseSelection()
        {
            MouseState mouseState = Mouse.GetState();
            if (tank != null && mouseState.LeftButton == ButtonState.Pressed)
            {
                Ray mouseRay = calculateRay(new Vector2(mouseState.X, mouseState.Y), camera.View, camera.Projection, game.GraphicsDevice.Viewport);
                float? distance = mouseRay.Intersects(new Plane(Vector3.Up, 0));
                if (distance != null)
                {
                    tank.Target = mouseRay.Position + mouseRay.Direction * (float)distance;
                }
            }
        }

        public Ray calculateRay(Vector2 mouseLocation, Matrix view, Matrix projection, Viewport viewport)
        {
            Vector3 nearPoint = viewport.Unproject(new Vector3(mouseLocation.X, mouseLocation.Y, 0.0f), projection, view, Matrix.Identity);
            Vector3 farPoint = viewport.Unproject(new Vector3(mouseLocation.X, mouseLocation.Y, 1.0f), projection, view, Matrix.Identity);
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            return new Ray(nearPoint, direction);
        }
        #endregion
    }
}