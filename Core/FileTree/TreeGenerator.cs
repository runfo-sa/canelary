using Core.Services;
using System.Collections.ObjectModel;

namespace Core.FileTree
{
    /// <summary>
    /// Genera el arbol de directorios, utilizar la propiedad Root para desplegar el arbol.
    /// </summary>
    public class TreeGenerator()
    {
        /// <summary>
        /// Raiz del arbol de directorios.
        /// </summary>
        public ObservableCollection<object> Root => InitTree();

        private readonly SettingsService _settings = SettingsService.Instance;
        private ObservableCollection<object>? _cachedRoot;

        private ObservableCollection<object> InitTree()
        {
            if (_cachedRoot != null)
            {
                return _cachedRoot;
            }

            List<VirtualDirectory> dirs = [new VirtualDirectory("Otros")];
            var files = VersionServiceProvider.Version.ListFiles();
            _cachedRoot = [];

            foreach (var dir in _settings.VirtualDirectories)
            {
                dirs.Add(new VirtualDirectory(dir.Name));
            }

            foreach (var file in files)
            {
                foreach (var dir in _settings.VirtualDirectories)
                {
                    if (file.Name.Contains(dir.Filter, StringComparison.CurrentCultureIgnoreCase))
                    {
                        dirs.First(d => d.Name == dir.Name).Files.Add(file);
                        goto OuterLoop; // Despues de agregar el archivo al directorio virtual saltamos al final del loop.
                    }
                }

                // Si no se pudo agregar el archivo a ningun directorio virtual definido, se lo asigna al directorio general "Otros".
                dirs[0].Files.Add(file);

            OuterLoop:
                continue;
            }

            var sortedDirs = dirs.OrderBy(d => d.Name);
            foreach (var dir in sortedDirs)
            {
                _cachedRoot.Add(dir);
            }

            return _cachedRoot;
        }
    }
}
