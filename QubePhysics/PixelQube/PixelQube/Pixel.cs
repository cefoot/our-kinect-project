using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelQube
{
    public class Pixel
    {
        public Pixel(float absorption = 0.0f)
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
        public object Tag { get; set; }
        /// <summary>
        /// Absorption for Speed less or equal 1
        /// <para>Speed is multiplied with this value</para>
        /// <para>1 means no absorption</para>
        /// <para>0 means instant absorption</para>
        /// </summary>
        public float Absorption { get; set; }
        public void Move()
        {
            Position += Speed;
            Speed *= Absorption;
        }
    }
}
