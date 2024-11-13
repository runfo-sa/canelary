using Core.FileTree;

namespace Core.Services
{
    /// <summary>
    /// Servicio que administra el sistema de control de versionado y almacenamiento de archivos.
    /// </summary>
    public interface IVersion
    {
        public IEnumerable<IFile> ListFiles(string version = "Local");

        public IEnumerable<string> ListVersions(IFile? file = null);

        public bool SaveFile(string path, string content);
    }
}