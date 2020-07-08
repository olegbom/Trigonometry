using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Trigonometry.Annotations;

namespace Trigonometry.ViewModels
{
    public enum TriangleEditingTool
    {
        Move,
        Rotate
    }

    public class TriangleEditorViewModel: INotifyPropertyChanged
    {
        
        public TriangleViewModel TriangleVm { get; set; }

        public TriangleEditingTool SelectedTool { get; set; } = TriangleEditingTool.Move;

        public bool IsMoveSelected
        {
            get => SelectedTool == TriangleEditingTool.Move;
            set
            {
                if(value)
                    SelectedTool = TriangleEditingTool.Move;
            }
        }

        public bool IsRotateSelected
        {
            get => SelectedTool == TriangleEditingTool.Rotate;
            set
            {
                 if (value) 
                     SelectedTool = TriangleEditingTool.Rotate;
            }
        }



        public TriangleEditorViewModel()
        {

        }





        


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
