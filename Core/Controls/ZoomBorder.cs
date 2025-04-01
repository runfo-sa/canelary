using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Core.Controls;

/// <summary>
/// Permite modificar el zoom y desplazamiento de las imagenes
/// <br/>
/// Fuente: <see href="https://stackoverflow.com/questions/741956/pan-zoom-image"/>
/// </summary>
public class ZoomBorder : Border
{
    private UIElement _child = null!;
    private Point _origin;
    private Point _start;

    private static TranslateTransform GetTranslateTransform(UIElement element)
    {
        return (TranslateTransform)((TransformGroup)element.RenderTransform)
          .Children.First(tr => tr is TranslateTransform);
    }

    private static ScaleTransform GetScaleTransform(UIElement element)
    {
        return (ScaleTransform)((TransformGroup)element.RenderTransform)
          .Children.First(tr => tr is ScaleTransform);
    }

    public override UIElement Child
    {
        get => base.Child;
        set
        {
            if (value != null && value != Child)
                Initialize(value);
            base.Child = value;
        }
    }

    public void Initialize(UIElement element)
    {
        _child = element;
        if (_child != null)
        {
            TransformGroup group = new();
            ScaleTransform st = new();
            group.Children.Add(st);
            TranslateTransform tt = new();
            group.Children.Add(tt);
            _child.RenderTransform = group;
            _child.RenderTransformOrigin = new Point(0.0, 0.0);
            MouseWheel += Child_MouseWheel;
            MouseLeftButtonDown += Child_MouseLeftButtonDown;
            MouseLeftButtonUp += Child_MouseLeftButtonUp;
            MouseMove += Child_MouseMove;
            PreviewMouseRightButtonDown += new MouseButtonEventHandler(
              Child_PreviewMouseRightButtonDown);
        }
    }

    public void Reset()
    {
        if (_child != null)
        {
            // reset zoom
            var st = GetScaleTransform(_child);
            st.ScaleX = 1.0;
            st.ScaleY = 1.0;

            // reset pan
            var tt = GetTranslateTransform(_child);
            tt.X = 0.0;
            tt.Y = 0.0;
        }
    }

    #region Child Events

    private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_child != null)
        {
            var st = GetScaleTransform(_child);
            var tt = GetTranslateTransform(_child);

            double zoom = e.Delta > 0 ? .2 : -.2;
            if ((e.Delta <= 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                return;

            Point relative = e.GetPosition(_child);
            double absoluteX;
            double absoluteY;

            absoluteX = relative.X * st.ScaleX + tt.X;
            absoluteY = relative.Y * st.ScaleY + tt.Y;

            st.ScaleX += zoom;
            st.ScaleY += zoom;

            tt.X = absoluteX - relative.X * st.ScaleX;
            tt.Y = absoluteY - relative.Y * st.ScaleY;
        }
    }

    private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_child != null)
        {
            var tt = GetTranslateTransform(_child);
            _start = e.GetPosition(this);
            _origin = new Point(tt.X, tt.Y);
            Cursor = Cursors.Hand;
            _child.CaptureMouse();
        }
    }

    private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_child != null)
        {
            _child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }
    }

    private void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Reset();
    }

    private void Child_MouseMove(object sender, MouseEventArgs e)
    {
        if (_child != null && _child.IsMouseCaptured)
        {
            var tt = GetTranslateTransform(_child);
            Vector v = _start - e.GetPosition(this);
            tt.X = _origin.X - v.X;
            tt.Y = _origin.Y - v.Y;
        }
    }

    #endregion Child Events
}