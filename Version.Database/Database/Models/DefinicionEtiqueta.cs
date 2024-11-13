using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace VersionDatabase.Db.Models
{
    [PrimaryKey(nameof(Id), nameof(IdEtiqueta))]
    public class DefinicionEtiqueta
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }

        public int IdEtiqueta { get; set; }

        public int Version { get; set; }
    }
}