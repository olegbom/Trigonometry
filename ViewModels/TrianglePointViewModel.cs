using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

using Trigonometry.Annotations;
using Vector2 = System.DoubleNumerics.Vector2;

namespace Trigonometry.ViewModels
{
    public class TrianglePointViewModel : INotifyPropertyChanged
    {
        public double X 
        {
            get => P.X;
            set => P = new Vector2(value, P.Y);
        }

        public double Y
        {
            get => P.Y; 
            set => P = new Vector2(P.X, value);
        }

        public Vector2 P { get; set; }

        public void OnPChanged()
        {
            Next.UpdateLength();
            Prev.UpdateLength();
            Triangle.SemiperimeterUpdate();
            UpdateAngle();
            Next.UpdateAngle();
            Prev.UpdateAngle();
        }


        #region Next

        public TrianglePointViewModel Next { get; set; }

        #endregion

        #region Prev

        public TrianglePointViewModel Prev { get; set; }

        #endregion

        public double Length { get; private set; }

        private void UpdateLength()
        {
            Length = (Next.P - Prev.P).Length();
        }

        public double Angle { get; private set; }

        private void UpdateAngle()
        {
            Angle = CalcAngleA(Triangle.Semiperimeter, Length, Next.Length, Prev.Length);
        }

        public double Degree => Angle * 180 / Math.PI;

        private static double CalcAngleA(double p, double a, double b, double c)
        {
            double k = (p - b) * (p - c) / p / (p - a);
            return 2 * Math.Atan(Math.Sqrt(k));
        }

        public void Deconstruct(out double x, out double y) 
        {
            x = P.X;
            y = P.Y;
        }

        public void Set(double x, double y) => P = new Vector2(x, y);


        public TriangleViewModel Triangle { get; set; }




        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
