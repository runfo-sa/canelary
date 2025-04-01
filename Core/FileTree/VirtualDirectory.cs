using System.Collections;
using System.Collections.ObjectModel;

namespace Core.FileTree;

/// <summary>
/// Directorio virtual para separar logicamente distinto tipos de archivos.
/// </summary>
public class VirtualDirectory(string name) : BindableBase
{
    /// <summary>
    /// Nombre del directorio virtual
    /// </summary>
    public string Name => name;

    /// <summary>
    /// Lista de archivos de este directorio
    /// </summary>
    public ObservableCollection<IFile> Files { get; set; } = [];

    /// <summary>
    /// Lista de subcarpetas de este directorio
    /// </summary>
    public ObservableCollection<VirtualDirectory> Subfolders { get; set; } = [];

    /// <summary>
    /// Lista de archivos recursivamente
    /// </summary>
    public IEnumerable RecursiveFiles => Subfolders!.Cast<object>().Concat(Files);
}