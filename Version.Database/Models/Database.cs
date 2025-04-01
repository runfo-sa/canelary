using System.IO;

using Core.FileTree;
using Core.Services;

using Version.Database.Models;

using VersionDatabase.Db;

namespace VersionDatabase.Models;

public class Database : IVersion
{
    public IEnumerable<IFile> ListFiles(string version = "Local")
    {
        using var context = new DatabaseDbContext();

        return context.Etiquetas
            .Select(e => new { e.Nombre, e.IdEtiqueta })
            .Distinct()
            .Select(n => new VirtualFile(n.Nombre, n.IdEtiqueta))
            .ToList();
    }

    public IEnumerable<string> ListVersions(IFile? file = null)
    {
        if (file is VirtualFile vfile)
        {
            return vfile.ListVersions();
        }

        var files = (IEnumerable<VirtualFile>)ListFiles();
        return files.First().ListVersions();
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
            var name = Path.GetFileName(path);
            return new VirtualFile(name, 0).Create(content);
        }
    }
}