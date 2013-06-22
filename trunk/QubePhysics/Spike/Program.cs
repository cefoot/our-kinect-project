using Microsoft.Xna.Framework;
using PixelQube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spike
{
    class Program
    {
        static void Main(string[] args)
        {
            var pnt1 = new Pixel()
            {
                Position = new Vector3(5f, 5f, 5f),
                //Speed = new Vector3(10f, 10f, 0f)
            };
            var pnt2 = new Pixel() { Position = new Vector3(-5f, -5f, -5f) };
            var lnk = new Link(pnt1, pnt2, distance: 1f);
            int i = 1;
            Print(pnt1, pnt2);
            do
            {
                Console.WriteLine("Step:{0}", i++);
                lnk.ApplyForce();
                Print(pnt1, pnt2);
                pnt1.Move();
                pnt2.Move();
            } while (pnt2.Speed.Length() > 0f || pnt1.Speed.Length() > 0f);
            Print(pnt1, pnt2);
            Console.WriteLine("Ready");
            Console.ReadKey();
        }

        private static void Print(Pixel pnt1, Pixel pnt2)
        {
            Console.WriteLine("Pnt1:Pos({0}) Speed({1})", pnt1.Position, pnt1.Speed);
            Console.WriteLine("Pnt2:Pos({0}) Speed({1})", pnt2.Position, pnt2.Speed);
            var dist = (pnt1.Position - pnt2.Position);
            Console.WriteLine("Distance:{0:00.00}||{1}", dist.Length(), dist);
        }
    }
}
