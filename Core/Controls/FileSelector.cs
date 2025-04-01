using System.Windows;
using System.Windows.Controls;

using Core.FileTree;

namespace Core.Controls;

/// <summary>
/// Sin esta clase no podriamos utilizar la interfaz <see cref="IFile"/> como plantilla en los archivos XAML.
/// Para su uso requiere ser incluido como recurso del archivo XAML.
/// </summary>
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