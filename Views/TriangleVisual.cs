using System;
using System.Collections.Generic;
using System.DoubleNumerics;
using System.Linq;
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
        private readonly DrawingVisual _visualGrid = new DrawingVisual();
        private readonly DrawingVisual _visualLines = new DrawingVisual();
        private readonly DrawingVisual[] _visualsPoints = new DrawingVisual[3];
        private readonly DrawingVisualSide[] _drawingVisualSides = new DrawingVisualSide[3];
        private readonly DrawingVisualPoint[] _drawingVisualPoints = new DrawingVisualPoint[3];

        private readonly List<EditableDrawingVisual> _editableDrawingVisuals = new List<EditableDrawingVisual>();

        private static readonly Typeface Typeface = new Typeface("Arial");


        private readonly Pen _gridPen = new Pen(new SolidColorBrush(Color.FromArgb(30,255,255,255)), 1);

        
        public static double Dpi { get; private set; } = 96.0;
        

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
                    triangleVisual.RemoveVisuals(oldTriangleVm);
                   
                }
                triangleVisual.CreateVisuals(triangleVm);

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
                    var geom = new StreamGeometry(){FillRule = FillRule.Nonzero};
                    using (var geomContext = geom.Open())
                    {
                        geomContext.BeginFigure(points[0], true, true);
                        geomContext.LineTo(points[1], true, false);
                        geomContext.LineTo(points[2], true, false);
                        
                    } 
                    context.DrawGeometry(new SolidColorBrush(Color.FromArgb(30, 255, 203, 0)), null,  geom);
                }

                foreach (var editableDrawingVisual in _editableDrawingVisuals)
                {
                    editableDrawingVisual.Draw();
                }

                    
            }
        }

        private void CreateVisuals(TriangleViewModel triangleVm)
        {
            for (int i = 0; i < 3; i++)
            {
                var newSide = new DrawingVisualSide(triangleVm[i], triangleVm[i + 1]);
                _drawingVisualSides[i] = newSide;
                _children.Add(newSide);
                _editableDrawingVisuals.Add(newSide);
            }
            for (int i = 0; i < 3; i++)
            { 
                var newPoint = new DrawingVisualPoint(triangleVm[i]);
                _drawingVisualPoints[i] = newPoint;
                _children.Add(newPoint);
                _editableDrawingVisuals.Add(newPoint);
            }
        }

        private void RemoveVisuals(TriangleViewModel triangleVm)
        {
            for (int i = 0; i < 3; i++)
            {
                _children.Remove(_drawingVisualSides[i]);
                _editableDrawingVisuals.Remove(_drawingVisualSides[i]);
                _drawingVisualSides[i] = null;

                _children.Remove(_drawingVisualPoints[i]);
                _editableDrawingVisuals.Remove(_drawingVisualPoints[i]);
                _drawingVisualPoints[i] = null;
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
            
            _children.Add(_visualGrid);
            _visualGrid.Transform = new TranslateTransform(0.5, 0.5);
            UpdateGrid();
            _children.Add(_visualLines);
            for (int i = 0; i < 3; i++)
            {
                _visualsPoints[i] = new DrawingVisual();
                _children.Add(_visualsPoints[i]);
            }

         
            Point downPoint = new Point(0,0);
            EditableDrawingVisual draggableDrawingVisual = null;
            MouseDown += (o, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Point pt = e.GetPosition(this);
                  
                    var dvp =  _editableDrawingVisuals.Find(x => x.IsMouseOver);
                    if ( dvp != null && VisualTreeHelper.HitTest(dvp, pt) != null)
                    {
                        dvp.StartDrag();
                        downPoint = pt;
                        draggableDrawingVisual = dvp;
                    }
                    

                }
            };
            MouseUp += (o, e) => 
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    draggableDrawingVisual = null;
                    TriangleVmOnPointsChanged(TriangleViewModel, null);
                }
            };
            MouseMove += (o, e) =>
            {
                Point pt = e.GetPosition(this);
                
                if (draggableDrawingVisual != null)
                {
                    Vector dv = pt - downPoint;
                    draggableDrawingVisual.Drag(new Vector2(dv.X, dv.Y));
                }
                else
                {
                    var edvHit = VisualTreeHelper.HitTest(this, pt)?.VisualHit as EditableDrawingVisual;
                    edvHit?.MouseOver(true);
                    foreach (var edvNotHit in _editableDrawingVisuals)
                    {
                        if(edvNotHit != edvHit)
                            edvNotHit.MouseOver(false);
                    }
                }

                
            };
                
            MouseLeave += (o, e) =>
            {
                TriangleVmOnPointsChanged(TriangleViewModel, null);
                draggableDrawingVisual = null;
            };

            SizeChanged += (o, e) =>
            {
                UpdateGrid();
            };

            Initialized += (o, e) =>
            {
                PresentationSource source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    Dpi = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                }
            };

        }


        private void UpdateGrid()
        {
            var size = RenderSize;
            
            using var context = _visualGrid.RenderOpen();
            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(255,30,30,30)), null, new Rect(size));
            for (int i = 1, count = (int) (size.Width / 10) + 1 ; i < count; i++)
            {
                context.DrawLine(_gridPen, new Point(i*10,0), new Point(i * 10, size.Height));
            }

            for (int i = 1, count = (int) (size.Height / 10) + 1; i < count; i++)
            {
                context.DrawLine(_gridPen, new Point(0, i*10), new Point(size.Width + 0.5, i*10) );
            }
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
