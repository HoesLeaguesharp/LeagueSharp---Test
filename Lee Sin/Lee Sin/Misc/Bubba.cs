using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Lee_Sin.Drawings;
using SharpDX;
using Color = System.Drawing.Color;

namespace Lee_Sin.Misc
{
    internal class Bubba
    {
        public static Vector2 RotateLineFromPoint(Vector2 point1, Vector2 point2, float value, bool radian = true)
        {
            var angle = !radian ? value*Math.PI/180 : value;
            var line = Vector2.Subtract(point2, point1);

            var newline = new Vector2
            {
                X = (float) (line.X*Math.Cos(angle) - line.Y*Math.Sin(angle)),
                Y = (float) (line.X*Math.Sin(angle) + line.Y*Math.Cos(angle))
            };

            return Vector2.Add(newline, point1);
        }

    }
}
