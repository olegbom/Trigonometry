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
        
    }
}
