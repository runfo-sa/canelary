using System.IO;

namespace Core.FileTree;

public class LabelFile(string path) : IFile
{
    public string Path => path;
    public string Name => System.IO.Path.GetFileName(path);

    public string Read()
    {
        return File.ReadAllText(Path);
    }

    public void Write(string content)
    {
        File.WriteAllText(Path, content);
    }
}