using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendTwins.Database.Model
{
    [PrimaryKey(nameof(Id))]
    public class Variable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Query { get; set; } = string.Empty;
    }
}
