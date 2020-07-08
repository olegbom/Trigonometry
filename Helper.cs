using System;
using System.Collections.Generic;
using System.DoubleNumerics;
using System.Text;
using System.Windows;
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
    }
}
