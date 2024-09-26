using Microsoft.EntityFrameworkCore;

namespace VersionDatabase.Db.Models
{
    [PrimaryKey(nameof(IdEtiqueta), nameof(IdLinea), nameof(Version))]
    public class FormatoEtiqueta
    {
        public int IdEtiqueta { get; set; }

        public int IdLinea { get; set; }

        public int Version { get; set; }

        public string Comandos { get; set; } = string.Empty;

        public string? Comentarios { get; set; }

        public bool? Habilitada { get; set; }
    }
}
