using Core.FileTree;

namespace Core.Services
{
    /// <summary>
    /// Servicio que administra el sistema de control de versionado y almacenamiento de archivos.
    /// </summary>
    public interface IVersion
    {
        public IEnumerable<IFile> ListFiles();

        public (IEnumerable<IFile>, IEnumerable<string>) FetchFileVer(IFile? file = null, string version = "Local");

        public bool SaveFile(string path, string content);

        public bool FetchByFile();

        public void Publish();
    }
}
