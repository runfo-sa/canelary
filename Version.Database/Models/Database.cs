using Core.FileTree;
using Core.Services;
using System.IO;
using Version.Database.Models;
using VersionDatabase.Db;

namespace VersionDatabase.Models
{
    public class Database : IVersion
    {
        public IEnumerable<IFile> ListFiles()
        {
            using var context = new DatabaseDbContext();
            return context.Etiquetas
                .Select(e => new { e.Nombre, e.IdEtiqueta })
                .Distinct()
                .Select(n => new VirtualFile(n.Nombre, n.IdEtiqueta))
                .ToList();
        }

        public void SaveFile(string path, string content)
        {
            if (!path.Any(Path.GetInvalidFileNameChars().Contains))
            {
                var split = path.Split(';');
                var name = split[0];
                var id = int.Parse(split[1]);
                new VirtualFile(name, id).Write(content);
            }
            else
            {
                new LabelFile(path).Write(content);
            }
        }

        public (IEnumerable<IFile>, IEnumerable<String>) FetchFileVer(IFile? file = null, String version = "Local")
        {
            var files = (IEnumerable<VirtualFile>)ListFiles();
            if (file is VirtualFile vfile)
            {
                return (files, vfile.ListVersions());
            }
            return (files, files.First().ListVersions());
        }

        public Boolean FetchByFile() => true;
    }
}
