using Core.FileTree;

namespace Core.Services
{
    public interface IVersion
    {
        public IEnumerable<string> ListVersions();

        public IEnumerable<IFile> ListFiles();

        public IEnumerable<IFile>? ListFiles(string version);

        public void SaveFile(string path, string content);
    }
}
