using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Database.IdeDbModels
{
    [PrimaryKey(nameof(Id))]
    public class RuleLabel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 1)]
        public int Id { get; set; }

        [ForeignKey(nameof(RuleId))]
        public int RuleId { get; set; }

        public string LabelName { get; set; } = string.Empty;
    }
}