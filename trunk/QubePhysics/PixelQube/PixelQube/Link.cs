using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PixelQube
{
    public class Link
    {
        private const float MaxStrength = 5f;
        private const float ForcePiece = 5f;

        public Link(Pixel pixl1, Pixel pixl2, float force = 1f, float distance = 0f)
        {
            Pixel1 = pixl1;
            Pixel2 = pixl2;
            Force = force;
            Distance = distance == 0f ? (Pixel1.Position - Pixel2.Position).Length() : distance;
            Color = Color.White;
            Debug.WriteLine(Distance);
        }

        public Color Color { get; set; }

        public void ApplyForce()
        {
            var distance = Pixel1.Position - Pixel2.Position;
            var distLength = distance.Length();
            if (distLength == Distance)
            {
                Color = Color.White;
                return;
            }
            var strength = Distance - distLength;
            ChangeColor(strength);
            distance.Normalize();
            Pixel1.Speed += distance * (strength / ForcePiece) * Force;
            Pixel2.Speed += -distance * (strength / ForcePiece) * Force;
        }

        private void ChangeColor(float strength)
        {
            var val = Math.Abs(strength);
            val = val > MaxStrength ? MaxStrength : val;
            if (strength < 0)
            {
                Color = new Color(byte.MaxValue - (byte)(byte.MaxValue / MaxStrength * val), Color.G, Color.B);
            }
            else
            {
                Color = new Color(Color.R, Color.G, byte.MaxValue - (byte)(byte.MaxValue / MaxStrength * val));
            }
        }
        public Pixel Pixel1 { get; private set; }
        public Pixel Pixel2 { get; private set; }
        public float Distance { get; private set; }
        public float Force { get; set; }

        public override int GetHashCode()
        {
            return Pixel1.GetHashCode() + Pixel2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var lObj = obj as Link;
            if (lObj == null)
            {
                return false;
            }
            var res = (Pixel1 == lObj.Pixel1 && Pixel2 == lObj.Pixel2) || (Pixel2 == lObj.Pixel1 && Pixel1 == lObj.Pixel2);
            return res;
        }
    }
}
