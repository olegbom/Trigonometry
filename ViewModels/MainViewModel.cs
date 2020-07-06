using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Trigonometry.Annotations;

namespace Trigonometry.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public TriangleViewModel TriangleVm { get; } = new TriangleViewModel();

        public MainViewModel()
        {
            TriangleVm[0].Set(10,10);
            TriangleVm[1].Set(200,10);
            TriangleVm[2].Set(100,160);
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
