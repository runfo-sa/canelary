using Core.FileTree;

namespace Core.Services
{
    public interface IVersion
    {
        public IEnumerable<IFile> ListFiles();

        public (IEnumerable<IFile>, IEnumerable<string>) FetchFileVer(IFile? file = null, string version = "Local");

        public void SaveFile(string path, string content);

        public bool FetchByFile();
    }
}
