using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNA3DView
{
    class Camera
    {
        private Vector3 position;
        private Vector3 target;
        public Matrix viewMatrix, projectionMatrix;
        private float yaw, pitch, roll;
        private float speed;
        private Matrix cameraRotation;
        public Camera()
        {
            ResetCamera();
        }
        

        private void ResetCamera()
        {
            position = new Vector3(0, 0, 500);
            target = new Vector3();

            yaw = 0.0f;
            pitch = 0.0f;
            roll = 0.0f;

            speed = .6f;

            cameraRotation = Matrix.Identity;

            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), 16 / 9, .5f, 5000f);
        }

        public void Update(Vector3 headPosition)
        {
            //HandleInput();
            
            UpdateViewMatrix(headPosition);
        }


        private void HandleInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.J))
            {
                yaw += .04f;
            }
            if (keyboardState.IsKeyDown(Keys.L))
            {
                yaw += -.04f;
            }
            if (keyboardState.IsKeyDown(Keys.I))
            {
                pitch += -.04f;
            }
            if (keyboardState.IsKeyDown(Keys.K))
            {
                pitch += .04f;
            }
            if (keyboardState.IsKeyDown(Keys.U))
            {
                roll += -.04f;
            }
            if (keyboardState.IsKeyDown(Keys.O))
            {
                roll += .04f;
            }

            if (keyboardState.IsKeyDown(Keys.W))
            {
                MoveCamera(cameraRotation.Forward);
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                MoveCamera(-cameraRotation.Forward);
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                MoveCamera(-cameraRotation.Right);
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                MoveCamera(cameraRotation.Right);
            }
            if (keyboardState.IsKeyDown(Keys.E))
            {
                MoveCamera(cameraRotation.Up);
            }
            if (keyboardState.IsKeyDown(Keys.Q))
            {
                MoveCamera(-cameraRotation.Up);
            }
        }

        private void MoveCamera(Vector3 addedVector)
        {
            position += speed * addedVector;
        }
        private void UpdateViewMatrix(Vector3 headPosition)
        {

            cameraRotation.Forward.Normalize();
            cameraRotation.Up.Normalize();
            cameraRotation.Right.Normalize();

            cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Right, pitch);
            cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Up, yaw);
            cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);

            yaw = 0.0f;
            pitch = 0.0f;
            roll = 0.0f;

            target = position + cameraRotation.Forward;

            //viewMatrix = Matrix.CreateLookAt(position, target, cameraRotation.Up);
            viewMatrix = Matrix.CreateLookAt(headPosition, target, cameraRotation.Up);
            //viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
        }

    }
}
