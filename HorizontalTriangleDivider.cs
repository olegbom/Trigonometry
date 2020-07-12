using System;
using System.Collections.Generic;
using System.DoubleNumerics;
using System.Linq;
using System.Text;


namespace Trigonometry
{
    public class VerticalQuadDivider
    {

        public readonly Vector2[] LeftPart;

        public readonly Vector2[] RightPart;

        public readonly Vector2[] CutPoints;


        public VerticalQuadDivider(Vector2[] quad)
        {
            var sX = quad.OrderBy(p => p.X).ToArray();
            

            // (x - sX[0].X)*(sX[2].Y - sX[0].Y) = (y - sX[0].Y)*(sY[2].X - sX[0].X);
            
            // 1

            double l = sX[1].X - sX[0].X;
            double c = sX[2].X - sX[1].X;
            double r = sX[3].X - sX[2].X;

            double lb02 = Helper.LinearApproximateY(sX[0], sX[2], sX[1].X);
            double lb03 = Helper.LinearApproximateY(sX[0], sX[3], sX[1].X);
            double lb = Math.Max(Math.Abs(lb02 - sX[1].Y), Math.Abs(lb03 - sX[1].Y));

            double rb03 = Helper.LinearApproximateY(sX[0], sX[3], sX[2].X);
            double rb13 = Helper.LinearApproximateY(sX[1], sX[3], sX[2].X);
            double rb = Math.Max(Math.Abs(rb03 - sX[2].Y), Math.Abs(rb13 - sX[2].Y));

            double sl = l * lb / 2;
            double sc = c * (lb + rb) / 2;
            double sr = r * rb / 2;

            double s = sl + sc + sr;

            double cutLevel = 0;
            if (sl > s / 2)
            {
                cutLevel = sX[0].X + Math.Sqrt(s*l/lb);

                //double y0 = Helper.LinearApproximateY(sX[0], sortY[1], cutLevel);
                //CutPoints[0] = new Vector2(cutLevel, y0);
                //double y1 = Helper.LinearApproximateY(sX[0], sortY[2], cutLevel);
                //CutPoints[1] = new Vector2(cutLevel, y1);

                //TopPart = new[] { sortY[0], CutPoints[0], CutPoints[1] };
                //BottomPart = new[] { CutPoints[0], CutPoints[1], sortY[2], sortY[1] };

            }
            else if (sr > s / 2)
            {
                cutLevel = sX[3].X - Math.Sqrt(s * r / rb);
            }
            else
            {
                double scl = s / 2 - sl;
                double scr = s / 2 - sr;

                double minb = Math.Min(lb, rb);
                double deltab = Math.Abs(lb - rb);


            }


        }
    }

    public class HorizontalTriangleDivider
    {

        public readonly Vector2[] TopPart;

        public readonly Vector2[] BottomPart;

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

                TopPart = new[] { sortY[0], CutPoints[0], CutPoints[1] };
                BottomPart = new[] { CutPoints[0], CutPoints[1], sortY[2], sortY[1] };
            }
            else
            {
                cutLevel = sortY[2].Y - Math.Sqrt(h * bh * (1 - factor));


                double x0 = Helper.LinearApproximateX(sortY[0], sortY[2], cutLevel);
                CutPoints[0] = new Vector2(x0, cutLevel);
                double x1 = Helper.LinearApproximateX(sortY[1], sortY[2], cutLevel);
                CutPoints[1] = new Vector2(x1, cutLevel);
                TopPart = new[] { CutPoints[0], CutPoints[1], sortY[1], sortY[0] };
                BottomPart = new[] { sortY[2], CutPoints[0], CutPoints[1] };
            }


        }

    }
}
