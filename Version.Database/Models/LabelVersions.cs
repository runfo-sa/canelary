using VersionDatabase.Db;
using VersionDatabase.Db.Models;

namespace Version.Database.Models
{
    public class LabelVersions
    {
        public string Name { get; set; }
        public IEnumerable<int> Versions { get; set; }
        public int CurrentVersion { get; set; }

        public LabelVersions(DefinicionEtiqueta etiqueta)
        {
            using var context = new DatabaseDbContext();
            Name = context.Etiquetas
                .Where(e => e.IdEtiqueta == etiqueta.IdEtiqueta)
                .First()
                .Nombre;

            CurrentVersion = etiqueta.Version;
            Versions = new VirtualFile(Name, CurrentVersion)
                .ListVersions()
                .Select(v => int.Parse(v));
        }
    }
}
