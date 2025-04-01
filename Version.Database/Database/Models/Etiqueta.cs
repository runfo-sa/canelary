using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace VersionDatabase.Db.Models;

[PrimaryKey(nameof(Id))]
public class Etiqueta
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 1)]
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int IdEtiqueta { get; set; }

    public int Version { get; set; }

    public DateTime? Fecha { get; set; }
}