using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DoubleNumerics;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Trigonometry.ViewModels;

namespace Trigonometry.Views
{
    public class TriangleVisual : FrameworkElement
    {
        private readonly VisualCollection _children;
        private readonly DrawingVisual _visualGrid = new DrawingVisual();
        private readonly DrawingVisual _visualCuttingForm = new DrawingVisual();

        private readonly List<EditableDrawingVisual> _editableDrawingVisuals = new List<EditableDrawingVisual>();

        private static readonly Typeface Typeface = new Typeface("Arial");


        private readonly Pen _gridPen = new Pen(new SolidColorBrush(Color.FromArgb(30,255,255,255)), 1);
        private readonly Pen _cuttingFormPen = new Pen(new SolidColorBrush(Color.FromArgb(211, 126, 255, 102)), 1);

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
                    triangleVisual.RemoveVisuals();
                   
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
                foreach (var editableDrawingVisual in _editableDrawingVisuals)
                {
                    editableDrawingVisual.Draw();
                }

                UpdateCuttingForm();
            }
        }

        private void CreateVisuals(TriangleViewModel triangleVm)
        {
            var newTriangle = new DrawingVisualTriangle(triangleVm);
            _children.Add(newTriangle);
            _editableDrawingVisuals.Add(newTriangle);
            for (int i = 0; i < 3; i++)
            {
                var newSide = new DrawingVisualSide(triangleVm[i], triangleVm[i + 1]);
                _children.Add(newSide);
                _editableDrawingVisuals.Add(newSide);
            }
            for (int i = 0; i < 3; i++)
            { 
                var newPoint = new DrawingVisualPoint(triangleVm[i]);
                _children.Add(newPoint);
                _editableDrawingVisuals.Add(newPoint);
            }
            _children.Add(_visualCuttingForm);
        }

        private void RemoveVisuals()
        {
            foreach (var edv in _editableDrawingVisuals)
            {
                _children.Remove(edv);
            }
            _editableDrawingVisuals.Clear();
            _children.Remove(_visualCuttingForm);
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
                  
                    for (int i = _editableDrawingVisuals.Count - 1; i >= 0 ; i--)
                    {
                        var edv = _editableDrawingVisuals[i];
                        if (edv.HitTest(pt) != null)
                        {
                            edv.MouseOver(true);
                            i--;
                            for (; i >= 0; i--)
                            {
                                edv = _editableDrawingVisuals[i];
                                edv.MouseOver(false);
                            }
                            break;
                        }
                        edv.MouseOver(false);
                    }
                }

                UpdateCuttingForm();


            };
            MouseWheel += (o, e) =>
            {
                Point pt = e.GetPosition(this);
                var dvp = _editableDrawingVisuals.Find(x => x.IsMouseOver);
                if (dvp != null && VisualTreeHelper.HitTest(dvp, pt) != null)
                {
                    dvp.Rotate(e.Delta, pt.ToVector2());
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

            Stopwatch sw = new Stopwatch();
            sw.Start();
            CompositionTarget.Rendering += (o, e) =>
            {
                _t = sw.ElapsedMilliseconds / 1000.0;
                UpdateCuttingForm();
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


        private double _t;

        private void UpdateCuttingForm()
        {
            if (TriangleViewModel == null)
                return;
            using var context = _visualCuttingForm.RenderOpen();
            
            
            
            Vector2[] points = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                points[i] = TriangleViewModel[i].P;
            }
            Vector2[] rotTri = new Vector2[3];
            double factor = 0.5  +  Math.Sin(_t)*0.5;

            
                
            HorizontalTriangleDivider divider = new HorizontalTriangleDivider(points, 1/3.0);
            context.DrawLine(_cuttingFormPen, divider.CutPoints[0].ToPoint(), divider.CutPoints[1].ToPoint());

            if (divider.BottomPart.Length == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    rotTri[i] = new Vector2(-divider.BottomPart[i].Y, divider.BottomPart[i].X);
                }
                divider = new HorizontalTriangleDivider(rotTri, 1 / 2.0);
                Vector2[] cutVertices = new Vector2[2];
                for (int i = 0; i < 2; i++)
                    cutVertices[i] = new Vector2(divider.CutPoints[i].Y, -divider.CutPoints[i].X);
                context.DrawLine(_cuttingFormPen, cutVertices[0].ToPoint(), cutVertices[1].ToPoint());
            }

           /* StreamGeometry geometry = new StreamGeometry { FillRule = FillRule.Nonzero };
            using (var geomContext = geometry.Open())
            {
                geomContext.BeginFigure(divider.CutPoints.Midpoint().ToPoint(), false, true);
                for (int i = 1; i < 360; i++)
                {
                    double phi = i * Math.PI / 180;
                    var rotMatrix = Matrix3x2.CreateRotation(phi);

                    for (int j = 0; j < 3; j++)
                    {
                        rotTri[j] = Vector2.Transform(points[j], rotMatrix);
                    }

                    divider = new HorizontalTriangleDivider(rotTri, factor);
                    Vector2 point = divider.CutPoints.Midpoint();
                    rotMatrix = Matrix3x2.CreateRotation(-phi);
                    point = Vector2.Transform(point, rotMatrix);
                    geomContext.LineTo(point.ToPoint(), true, false);
                }
            }
            context.DrawGeometry(Brushes.Transparent, _cuttingFormPen, geometry);*/
            
        }

        private double GetArea(Vector2[] va, int count)
        {
            Vector2[] topSorted = va;
            if (count > 3)
            {
                var topCenter = Vector2.Zero;
                for (int i = 0; i < count; i++)
                    topCenter += va[i];
                topCenter /= count;
                topSorted = va.OrderBy(p => (p - topCenter).PseudoPhase()).ToArray();
            }
            var result = topSorted.GetArea(0, count);
            return result;
        }




       
    
   



        public Geometry GetPolygon(Vector2[] vertices)
        {
            var geom = new StreamGeometry { FillRule = FillRule.Nonzero };
            using (var geomContext = geom.Open())
            {
                geomContext.BeginFigure(vertices[0].ToPoint(), true, true);
                for (int i = 1; i < vertices.Length; i++)
                    geomContext.LineTo(vertices[i].ToPoint(), true, false);
            }

            return geom;
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
