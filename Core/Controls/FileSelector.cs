using Core.FileTree;
using System.Windows;
using System.Windows.Controls;

namespace Core.Controls
{
    public class FileSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IFile)
            {
                return (DataTemplate)((FrameworkElement)container).FindResource("fileTemplate");
            }
            return base.SelectTemplate(item, container);
        }
    }
}
