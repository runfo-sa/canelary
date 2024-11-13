namespace Core.FileTree
{
    public interface IFile
    {
        public string Path { get; }
        public string Name { get; }

        public string Read();

        public void Write(string content);
    }
}