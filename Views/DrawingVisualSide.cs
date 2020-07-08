using System;
using System.DoubleNumerics;
using System.Windows.Media;
using Trigonometry.ViewModels;

namespace Trigonometry.Views
{
    public class DrawingVisualSide : EditableDrawingVisual
    {
        //private new static readonly Pen Pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0xf0, 0xC4, 0x19)), 1);
        private static readonly Pen SelectedPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0xf0, 0xC4, 0x19)), 3);

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

        public override void Rotate(int delta, Vector2 mousePos)
        {
            var rotMat = Matrix3x2.CreateRotation(delta/90.0 / 180.0 * Math.PI, mousePos);
            _firstPoint.P = Vector2.Transform(_firstPoint.P, rotMat);
            _secondPoint.P = Vector2.Transform(_secondPoint.P, rotMat);

        }


        public override void Draw()
        {
            using var context = RenderOpen();
            context.DrawLine(IsMouseOver ? SelectedPen : Pen, _firstPoint.P.ToPoint(), _secondPoint.P.ToPoint());
        }
    }
}