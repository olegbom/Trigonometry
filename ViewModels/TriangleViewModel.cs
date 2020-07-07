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
        public TrianglePointViewModel[] Points { get; } = new TrianglePointViewModel[3]
        {
            new TrianglePointViewModel(),
            new TrianglePointViewModel(),
            new TrianglePointViewModel(),
        };

        public TrianglePointViewModel this[int i] => Points[i % 3];

        public double Semiperimeter { get; private set; }

        public void SemiperimeterUpdate()
        {
            Semiperimeter = (this[0].Length + this[1].Length + this[2].Length) / 2;
        }
        
        public double Indent { get; set; } = 10;

        public TriangleViewModel()
        {
            for (int i = 0; i < 3; i++)
            {
                this[i].Next = this[i + 1];
                this[i].Prev = this[i + 2];
                this[i].Triangle = this;
            }
            
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
