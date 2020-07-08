using System;
using System.DoubleNumerics;
using System.Windows.Media;
using Trigonometry.ViewModels;

namespace Trigonometry.Views
{
    public class DrawingVisualTriangle : EditableDrawingVisual
    {
        private static readonly Brush Fill = new SolidColorBrush(Color.FromArgb(30, 255, 203, 0));
        private static readonly Brush SelectedFill = new SolidColorBrush(Color.FromArgb(114, 255, 203, 0));


        private readonly TriangleViewModel _triangleVm;

        public DrawingVisualTriangle(TriangleViewModel triangleVm)
        {
            _triangleVm = triangleVm;
        }

        public override void StartDrag()
        {
            _triangleVm.StartDrag();
        }

        public override void Drag(Vector2 dv)
        {
            _triangleVm.Drag(dv);
        }

        public override void Rotate(int delta, Vector2 mousePos)
        {
            _triangleVm.Rotate(delta/90.0 / 180.0 * Math.PI, mousePos);
        }

        public override void Draw()
        {
            using var context = RenderOpen();
            var geom = new StreamGeometry { FillRule = FillRule.Nonzero };
            using (var geomContext = geom.Open())
            {
                geomContext.BeginFigure(_triangleVm[0].P.ToPoint(), true, true);
                geomContext.LineTo(_triangleVm[1].P.ToPoint(), true, false);
                geomContext.LineTo(_triangleVm[2].P.ToPoint(), true, false);
            }
            context.DrawGeometry( IsMouseOver ? SelectedFill : Fill, null, geom );
        }
    }
}