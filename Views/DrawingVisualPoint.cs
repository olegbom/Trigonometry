using System;
using System.ComponentModel;
using System.DoubleNumerics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Trigonometry.ViewModels;

namespace Trigonometry.Views
{
    public class DrawingVisualPoint : EditableDrawingVisual
    {
        private readonly TrianglePointViewModel _pointVm;
        private FormattedText _labelFormattedText;


        public DrawingVisualPoint(TrianglePointViewModel pointVm)
        {
            _pointVm = pointVm;
            _pointVm.PropertyChanged += PointVmOnPropertyChanged;
            PointVmOnPropertyChanged(null, new PropertyChangedEventArgs(nameof(TrianglePointViewModel.Label)));
        }

        private void PointVmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrianglePointViewModel.Label))
            {
                _labelFormattedText = new FormattedText(_pointVm.Label, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, Typeface,
                    14, Brushes.Beige, TriangleVisual.Dpi);
            }
        }

        protected override void MouseOverChanged()
        {
            Draw();
        }


        private Vector2 _oldMouseDown = Vector2.Zero;
        public override void StartDrag()
        {
            _oldMouseDown = _pointVm.P;
        }

        public override void Drag(Vector2 dv)
        {
            Vector2 newP = _oldMouseDown + dv;
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                newP = new Vector2(Math.Round(newP.X / 10) * 10, Math.Round(newP.Y / 10) * 10);
            }

            _pointVm.P = newP;
        }


        public override void Draw()
        {
            using var context = RenderOpen();
            Point p = _pointVm.P.ToRoundedPoint(); 

            context.DrawEllipse(IsMouseOver ? Brushes.Gold : new SolidColorBrush(Color.FromRgb(30,30,30)), Pen, p, 5, 5);

            var labelPos = _pointVm.GetLabelPos() - new Vector2(_labelFormattedText.Width / 2, _labelFormattedText.Height / 2);
            context.DrawText(_labelFormattedText, labelPos.ToPoint());
        }
        
    }
}