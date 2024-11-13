using VersionDatabase.Db;
using VersionDatabase.Db.Models;

namespace Version.Database.Models
{
    public class LabelVersions
    {
        public DefinicionEtiqueta Etiqueta { get; set; }
        public string Name { get; set; }
        public IEnumerable<int> Versions { get; set; }
        public int SelectedVersion { get; set; }
        public int CurrentVersion { get; set; }

        public LabelVersions(DefinicionEtiqueta etiqueta)
        {
            using var context = new DatabaseDbContext();

            Name = context.Etiquetas
                .Where(e => e.IdEtiqueta == etiqueta.IdEtiqueta)
                .First()
                .Nombre;

            CurrentVersion = etiqueta.Version;
            SelectedVersion = etiqueta.Version;

            Versions = new VirtualFile(Name, etiqueta.IdEtiqueta)
                .ListVersions()
                .Select(v => int.Parse(v));

            Etiqueta = etiqueta;
        }
    }
}