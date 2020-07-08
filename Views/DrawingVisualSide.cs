using System.DoubleNumerics;
using System.Windows.Media;
using Trigonometry.ViewModels;

namespace Trigonometry.Views
{
    public class DrawingVisualSide : EditableDrawingVisual
    {
        private static readonly Pen SelectedPen = new Pen(Brushes.Black, 3);

        private readonly TrianglePointViewModel _firstPoint;
        private readonly TrianglePointViewModel _secondPoint;

        public DrawingVisualSide(TrianglePointViewModel firstPoint, TrianglePointViewModel secondPoint)
        {
            _firstPoint = firstPoint;
            _secondPoint = secondPoint;
        }

        private Vector2 _oldMouseDownFirst = Vector2.Zero;
        private Vector2 _oldMouseDownSecond = Vector2.Zero;
        public override void StartDrag()
        {
            _oldMouseDownFirst = _firstPoint.P;
            _oldMouseDownSecond = _secondPoint.P;
        }

        public override void Drag(Vector2 dv)
        {
            _firstPoint.P = _oldMouseDownFirst + dv;
            _secondPoint.P = _oldMouseDownSecond + dv;
        }

        protected override void MouseOverChanged()
        {
            Draw();
        }

        public override void Draw()
        {
            using var context = RenderOpen();
            context.DrawLine(IsMouseOver ? SelectedPen : Pen, _firstPoint.P.ToRoundedPoint(), _secondPoint.P.ToRoundedPoint());
        }
    }
}