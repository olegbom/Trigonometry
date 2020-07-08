using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DoubleNumerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Trigonometry.Annotations;

namespace Trigonometry.ViewModels
{
    public class TriangleViewModel: INotifyPropertyChanged
    {
        private static readonly string[] Labels = {"A", "B", "C"};

        private readonly TrianglePointViewModel[] _points = new TrianglePointViewModel[3];

        public TrianglePointViewModel this[int i] => _points[i % 3];

        public double Semiperimeter { get; private set; }

        public void SemiperimeterUpdate()
        {
            Semiperimeter = (this[0].Length + this[1].Length + this[2].Length) / 2;
        }

        public double Area { get; private set; }

        public void AreaUpdate()
        {
            double area = 0;
            for (int i = 0; i < 3; i++)
            {
                area += (this[i].X + this[i + 1].X) * (this[i].Y - this[i + 1].Y);
            }
            area /= 2;
            Area = Math.Abs(area);
        }
        
        public double Indent { get; set; } = 15;


        public bool IsPointsReactionEnabled { get; set; } = true;

        public TriangleViewModel()
        {
            for(int i = 0; i < 3; i++)
                _points[i] = new TrianglePointViewModel(this, Labels[i]);

            for (int i = 0; i < 3; i++)
            {
                this[i].Next = this[i + 1];
                this[i].Prev = this[i + 2];
            }
            
        }

        private Vector2[] _oldPoints = new Vector2[3]{Vector2.Zero, Vector2.Zero, Vector2.Zero}; 
        public void StartDrag()
        {
            for (int i = 0; i < 3; i++)
            {
                _oldPoints[i] = _points[i].P;
            }
        }

        public void Drag(Vector2 dv)
        {
            IsPointsReactionEnabled = false;
            for (int i = 0; i < 3; i++)
                _points[i].P = _oldPoints[i] + dv;
            IsPointsReactionEnabled = true;
            
            InvokePointsChanged();
        }

        public void Rotate(double rad, Vector2 origPoint)
        {
            var rotMat = Matrix3x2.CreateRotation(rad, origPoint);
            IsPointsReactionEnabled = false;
            for (int i = 0; i < 3; i++)
                _points[i].P = Vector2.Transform(_points[i].P, rotMat);
            IsPointsReactionEnabled = true;
           
            InvokePointsChanged();
        }


        public event EventHandler PointsChanged;

        public void InvokePointsChanged()
        {
            PointsChanged?.Invoke(this, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
