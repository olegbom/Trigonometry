using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DoubleNumerics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Trigonometry.ViewModels;

namespace Trigonometry.Views
{
    public class TriangleVisual : FrameworkElement
    {


        private readonly VisualCollection _children;
        private readonly DrawingVisual _visualLines = new DrawingVisual();
        private readonly DrawingVisual[] _visualsPoints = new DrawingVisual[3];

        private readonly Pen _pen = new Pen(Brushes.Black, 1);
        private readonly Pen _anglePen = new Pen(Brushes.DodgerBlue, 1);
        private readonly Typeface _typeface = new Typeface("Arial");

        private double _dpi = 96.0;


        public static readonly DependencyProperty TriangleViewModelProperty = DependencyProperty.Register(
            "TriangleViewModel", typeof(TriangleViewModel), typeof(TriangleVisual),
            new PropertyMetadata(default(TriangleViewModel), TriangleChangedCallback));

        private static void TriangleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TriangleViewModel triangleVm &&
                d is TriangleVisual triangleVisual)
            {
                if (e.OldValue is TriangleViewModel oldTriangleVm)
                {
                    oldTriangleVm.PointsChanged -= triangleVisual.TriangleVmOnPointsChanged;
                }

                triangleVm.PointsChanged += triangleVisual.TriangleVmOnPointsChanged;
                triangleVisual.TriangleVmOnPointsChanged(triangleVm, null);
            }
        }

        private void TriangleVmOnPointsChanged(object sender, EventArgs e)
        {
            if (sender is TriangleViewModel triangleVm)
            {
                Point[] points = new Point[3];
                for (int i = 0; i < 3; i++)
                    points[i] = new Point(Math.Round(triangleVm[i].X) + 0.5, Math.Round(triangleVm[i].Y) + 0.5);
                using (var context = _visualLines.RenderOpen())
                {
                    context.DrawRectangle(Brushes.Beige, null, new Rect(RenderSize));
                    for (int i = 0; i < 3; i++)
                        context.DrawLine(_pen, points[i], points[(i + 1) % 3]);
                }

                for (int i = 0; i < 3; i++)
                {
                    using var context = _visualsPoints[i].RenderOpen();
                    context.DrawEllipse(Brushes.White, _pen, points[i], 5, 5);
                    
                    
                    var text = new FormattedText(triangleVm[i].Label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeface,
                        14, Brushes.Black, _dpi);
                    var labelPos = triangleVm[i].GetLabelPos() - new Vector2(text.Width/2, text.Height/2);
                    context.DrawText(text, labelPos.ToPoint());

                    
                    
                    //var drawing = new GeometryDrawing(Brushes.Transparent, _anglePen, new PathGeometry( new[]{new PathFigure() }));
                }
                    
            }
        }


        public TriangleViewModel TriangleViewModel
        {
            get => (TriangleViewModel) GetValue(TriangleViewModelProperty);
            set => SetValue(TriangleViewModelProperty, value);
        }


        public TriangleVisual()
        {
            
            _children = new VisualCollection(this);

            _children.Add(_visualLines);
            for (int i = 0; i < 3; i++)
            {
                _visualsPoints[i] = new DrawingVisual();
                _children.Add(_visualsPoints[i]);
            }

            bool isPressed = false;
            int pressedPoint = 0;
            Point downPoint = new Point(0,0);
            Vector2 oldPosPoint = Vector2.Zero;
            MouseDown += (o, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Point pt = e.GetPosition(this);
                    for (int i = 0; i < 3; i++)
                    {
                        if (VisualTreeHelper.HitTest(_visualsPoints[i], pt) != null)
                        {
                            downPoint = pt;
                            oldPosPoint = TriangleViewModel[i].P;
                            isPressed = true;
                            pressedPoint = i;
                            break;
                        }
                    }

                }
            };
            MouseUp += (o, e) => 
            { 
                if (e.ChangedButton == MouseButton.Left) 
                    isPressed = false;
            };
            MouseMove += (o, e) =>
            {
                if (isPressed)
                {
                    Point pt = e.GetPosition(this);
                    Vector dv = pt - downPoint;
                    TriangleViewModel[pressedPoint].P = oldPosPoint + new Vector2(dv.X, dv.Y);

                }
            };
                
            MouseLeave += (o, e) => { isPressed = false; };

            SizeChanged += (o, e) => { TriangleVmOnPointsChanged(TriangleViewModel, null); };

            Initialized += (o, e) =>
            {
                PresentationSource source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    _dpi = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                }
            };

        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            

        }


        protected override int VisualChildrenCount => _children.Count;

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }
}
