using System;
using System.Collections.Generic;
using System.DoubleNumerics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Trigonometry.Annotations;

namespace Trigonometry
{
    public static class Helper
    {
        public static void Deconstruct(this Point p, out double x, out double y)
        {
            x = p.X;
            y = p.Y;
        }

        public static Point ToPoint(this Vector2 v) => new Point(v.X, v.Y);
        public static Point ToRoundedPoint(this Vector2 v) => new Point(Math.Round(v.X) + 0.5, Math.Round(v.Y) + 0.5);

        public static Vector2 ToVector2(this Point p) => new Vector2(p.X, p.Y);


        public static double PseudoPhase(this Vector2 v) => PseudoAtan2(v.Y, v.X);

        public static  double PseudoAtan2(double y, double x)
        {
            if (Math.Abs(y) < 1e-10 && Math.Abs(x) < 1e-10)
            {
                return 0;
            }

            if (Math.Abs(y) < 1e-10)
            {
                return (x > 0) ? 0 : 2;
            }

            if (Math.Abs(x) < 1e-10)
            {
                return (y > 0) ? 1 : 3;
            }

            if (x > 0 && y > 0) // 1 quadrant
            {
                //x = 1 - y;
                //x = y*xi/yi;
                //y - ?
                //1 - y = y*xi/yi
                // y * (xi/yi + 1) = 1
                // y = 1/(xi/y1 + 1);
                return 1 / (1 + x / y);
            }
            if (x < 0 && y > 0) // 2 quadrant
            {
                return 1 + 1 / (1 - y / x);
            }
            if (x < 0 && y < 0) // 3 quadrant
            {
                return 2 + 1 / (1 + x / y);
            }
            // 4 quadrant
            return 3 + 1 / (1 - y / x);
        }

        public static double GetArea(this Vector2[] vertices, int offset, int length)
        {
            if(length > vertices.Length)
                throw new ArgumentOutOfRangeException("length >  vertices.Length");
            if(length + offset > vertices.Length)
                throw new ArgumentOutOfRangeException("length + offset > vertices.Length");

            double result = 0;

            int ending = offset + length;
            for (int i = offset; i < ending; i++)
            {
                var a = vertices[i];
                int nextIndex = i + 1;
                nextIndex = (nextIndex == ending) ? offset : nextIndex;
                var b = vertices[nextIndex];
                result += (a.X + b.X) * (a.Y - b.Y);
            }
            
            return Math.Abs(result/2);
        }

        public static double GetArea(this Vector2[] vertices)
        {
            return vertices.GetArea(0, vertices.Length);
        }

        public static Vector2 Midpoint(this Vector2[] vertices)
        {
            Vector2 result = Vector2.Zero;
            for (int i = 0; i < vertices.Length; i++)
            {
                result += vertices[i];
            }
            result /= vertices.Length;
            return result;
        }


        public static double LinearApproximateX(Vector2 a, Vector2 b, double y)
        {
            return (y - a.Y) * (b.X - a.X) / (b.Y - a.Y) + a.X;
        }
        public static double LinearApproximateY(Vector2 a, Vector2 b, double x)
        {
            return (x - a.X) * (b.Y - a.Y) / (b.X - a.X) + a.Y;
        }

    }
}
