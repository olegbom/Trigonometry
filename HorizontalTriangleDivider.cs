using System;
using System.Collections.Generic;
using System.DoubleNumerics;
using System.Linq;
using System.Text;

namespace Trigonometry
{
    public class VerticalQuadDivider
    {
        public VerticalQuadDivider(Vector2[] quad)
        {
            var sX = quad.OrderBy(p => p.X).ToArray();
            

            // (x - sX[0].X)*(sX[4].Y - sX[0].Y) < (y - sX[0].Y)*(sY[4].X - sX[0].X);
            
            // 1

            double l = sX[1].X - sX[0].X;
            double c = sX[2].X - sX[1].X;
            double r = sX[3].X - sX[2].X;           


        }
    }

    public class HorizontalTriangleDivider
    {

        public readonly Vector2[] TopTriangle;

        public readonly Vector2[] BottomTriangle;

        public readonly Vector2[] CutPoints;
        

        public HorizontalTriangleDivider(Vector2[] tri, double factor)
        {
            var sortY = tri.OrderBy(p => p.Y).ToArray();
            double th = sortY[1].Y - sortY[0].Y;
            double bh = sortY[2].Y - sortY[1].Y;
            double h = th + bh;

            double cutLevel = 0;
            CutPoints = new Vector2[2];

            if (th > h * factor)
            {
                cutLevel = sortY[0].Y + Math.Sqrt(h * th * factor);

                double x0 = Helper.LinearApproximateX(sortY[0], sortY[1], cutLevel);
                CutPoints[0] = new Vector2(x0, cutLevel);
                double x1 = Helper.LinearApproximateX(sortY[0], sortY[2], cutLevel);
                CutPoints[1] = new Vector2(x1, cutLevel);

                TopTriangle = new[] { sortY[0], CutPoints[0], CutPoints[1] };
                BottomTriangle = new[] { CutPoints[0], CutPoints[1], sortY[2], sortY[1] };
            }
            else
            {
                cutLevel = sortY[2].Y - Math.Sqrt(h * bh * (1 - factor));


                double x0 = Helper.LinearApproximateX(sortY[0], sortY[2], cutLevel);
                CutPoints[0] = new Vector2(x0, cutLevel);
                double x1 = Helper.LinearApproximateX(sortY[1], sortY[2], cutLevel);
                CutPoints[1] = new Vector2(x1, cutLevel);
                TopTriangle = new[] { CutPoints[0], CutPoints[1], sortY[1], sortY[0] };
                BottomTriangle = new[] { sortY[2], CutPoints[0], CutPoints[1] };
            }


        }

    }
}
