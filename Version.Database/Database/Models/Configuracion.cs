using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace VersionDatabase.Db.Models
{
    [PrimaryKey(nameof(Id))]
    public class Configuracion
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Valor { get; set; }
    }
}
