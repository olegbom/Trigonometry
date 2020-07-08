using System.DoubleNumerics;
using System.Windows.Media;

namespace Trigonometry.Views
{
    public abstract class EditableDrawingVisual : DrawingVisual
    {
        protected static readonly Pen Pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0xf0, 0xC4, 0x19)), 1);
        protected static readonly Typeface Typeface = new Typeface("Arial");

        public bool IsMouseOver { get; private set; }

        public void MouseOver(bool isMouseOver)
        {
            if (isMouseOver != IsMouseOver)
            {
                IsMouseOver = isMouseOver;
                MouseOverChanged();
            }
        }

        public abstract void StartDrag();
        public abstract void Drag(Vector2 dv);

        protected virtual void MouseOverChanged()
        {
            Draw();
        }

        public virtual void Rotate(int delta, Vector2 mousePos) { }

        public abstract void Draw();

    }
}