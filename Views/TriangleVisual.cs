using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DoubleNumerics;
using System.Globalization;
using System.Linq;
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

                UpdateCuttingForm(_mousePos);
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

                UpdateCuttingForm(pt);


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

        private Point _mousePos;
        private void UpdateCuttingForm(Point mousePos)
        {
            _mousePos = mousePos;
            using var context = _visualCuttingForm.RenderOpen();
            
            
            
            Vector2[] points = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                points[i] = TriangleViewModel[i].P;
            }
            
            var cutLevel = DivideTriangleHorizontal(points, 1 / 3.0);


           
            var cutVertices = new Vector2[2];
            int countOfVertices = 0;
            for (int i = 0; i < 3; i++)
            {
                var a = TriangleViewModel[i].P;
                var b = TriangleViewModel[i + 1].P;
                if ((a.Y <= cutLevel && b.Y >  cutLevel) ||
                    (a.Y >  cutLevel && b.Y <= cutLevel))
                {
                    //(x - a.X) / (b.X - a.X) = (y - a.Y) / (b.Y - a.Y);
                    //(x - a.X) * (b.Y - a.Y) = (y - a.Y) * (b.X - a.X);
                    //выражаем X
                    //x = (y - a.Y) * (b.X - a.X) / (b.Y - a.Y) + a.X;
                    var x = a.X;
                    if (Math.Abs(b.Y - a.Y) > 1e-6)
                    {
                        x = (cutLevel - a.Y) * (b.X - a.X) / (b.Y - a.Y) + a.X;
                    }

                    var vertex = new Vector2(x, cutLevel);
                    context.DrawEllipse(Brushes.GreenYellow, null, new Point(x, cutLevel), 4, 4);
                    cutVertices[countOfVertices] = vertex;
                    countOfVertices++;
                }
            }
            
            if (countOfVertices == 2)
            {
                context.DrawLine(_cuttingFormPen, cutVertices[0].ToRoundedPoint(), cutVertices[1].ToRoundedPoint());
                Vector2[] topPartOfTriangle = new Vector2[4];
                Vector2[] bottomPartOfTriangle = new Vector2[4];
                int numberOfVerticesTop = 2;
                int numberOfVerticesBottom = 2;
                bottomPartOfTriangle[0] = topPartOfTriangle[0] = cutVertices[0];
                bottomPartOfTriangle[1] = topPartOfTriangle[1] = cutVertices[1];

                for (int i = 0; i < 3; i++)
                {
                    var p = TriangleViewModel[i].P;
                    if (p.Y <= cutLevel)
                    {
                        topPartOfTriangle[numberOfVerticesTop] = p;
                        numberOfVerticesTop++;
                    }
                    else
                    {
                        bottomPartOfTriangle[numberOfVerticesBottom] = p;
                        numberOfVerticesBottom++;
                    }
                }

                var midCutPoint = (cutVertices[0] + cutVertices[1]) / 2;
                var areaTop = GetArea(topPartOfTriangle, numberOfVerticesTop);
                var textTop = new FormattedText($"{areaTop/TriangleViewModel.Area*100:F2}%", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, 14, Brushes.Beige, Dpi);
                context.DrawText(textTop, (midCutPoint + new Vector2(-textTop.Width / 2, -25)).ToRoundedPoint());


                if (numberOfVerticesBottom == 3)
                {

                }
                var areaBottom = GetArea(bottomPartOfTriangle, numberOfVerticesBottom);
                var textBottom = new FormattedText($"{areaBottom / TriangleViewModel.Area * 100:F2}%", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, 14, Brushes.Beige, Dpi);
                context.DrawText(textBottom, (midCutPoint + new Vector2(-textBottom.Width / 2, 25)).ToRoundedPoint());
            }


            
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

        private double DivideTriangleHorizontal(Vector2[] tri, double factor)
        {
            var sortY = tri.OrderBy(p => p.Y).ToArray();

            double th = sortY[1].Y - sortY[0].Y;
            double bh = sortY[2].Y - sortY[1].Y;
            double h = th + bh;

            if (th > h*factor)
                return sortY[0].Y + Math.Sqrt((th + bh) * th * factor);
            return sortY[2].Y - Math.Sqrt( (th + bh) * bh*(1 - factor));
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
