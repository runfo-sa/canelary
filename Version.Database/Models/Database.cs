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

        public bool SaveFile(string path, string content)
        {
            if (path.Contains('@'))
            {
                var split = path.Split('@');
                var name = split[0];
                var id = int.Parse(split[1]);
                new VirtualFile(name, id).Write(content);
                return true;
            }
            else
            {
                var name = Path.GetFileNameWithoutExtension(path);
                return new VirtualFile(name, 0).Create(content);
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

        public void Publish()
        {
            throw new NotImplementedException();
        }
    }
}
