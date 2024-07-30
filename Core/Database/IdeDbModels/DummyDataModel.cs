using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Database.IdeDbModels
{
    /// <summary>
    /// Representación del estado de un cliente en una tabla.
    /// </summary>
    [PrimaryKey(nameof(Id))]
    public class DummyDataModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }

        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
