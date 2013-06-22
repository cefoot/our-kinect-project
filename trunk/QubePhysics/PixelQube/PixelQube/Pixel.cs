using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PixelQube
{
    public class Pixel
    {
        public Pixel(float absorption = 0.5f)
        {
            Absorption = absorption;
            Color = Color.White;
        }
        /// <summary>
        /// Position of Pixel
        /// </summary>
        public Vector3 Position { get { return _drawPixel.Position; } set { _drawPixel.Position = value; } }

        public Color Color { get { return _drawPixel.Color; } set { _drawPixel.Color = value; } }

        VertexPositionColor _drawPixel = new VertexPositionColor();
        public VertexPositionColor DrawPixel
        {
            get
            {
                return _drawPixel;
            }
        }

        /// <summary>
        /// Speed of Pixel
        /// </summary>
        public Vector3 Speed { get; set; }
        public Vector3 CurrentGravity { get; set; }
        public object Tag { get; set; }
        /// <summary>
        /// Absorption for Speed less or equal 1
        /// <para>Speed is multiplied with this value</para>
        /// <para>1 means no absorption</para>
        /// <para>0 means instant absorption</para>
        /// </summary>
        public float Absorption { get; set; }
        public void Move(Vector3 gravity, params Plane[] walls)
        {
            bool collide = false;
            foreach (var wall in walls)
            {
                var curDist = wall.DotCoordinate(Position);
                if (curDist > 0)
                {
                    collide = true;
                    Speed = new Vector3(
                        wall.Normal.X == 0 ? Speed.X : wall.Normal.X * Speed.X,
                        wall.Normal.Y == 0 ? Speed.Y : wall.Normal.Y * Speed.Y,
                        wall.Normal.Z == 0 ? Speed.Z : wall.Normal.Z * Speed.Z
                        );
                    CurrentGravity = Vector3.Zero;
                }
            }
            if (Speed.Y > 0 && Speed.Y > Math.Abs(gravity.Y))
            {
                CurrentGravity = Vector3.Zero;
            }
            Position += Speed + CurrentGravity;
            CurrentGravity += gravity * Absorption;
            Speed *= Absorption;
        }
    }
}
