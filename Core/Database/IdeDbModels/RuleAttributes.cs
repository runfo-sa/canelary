using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

namespace Core.Database.IdeDbModels;

[PrimaryKey(nameof(Id))]
public class RuleAttributes
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 1)]
    public int Id { get; set; }

    [ForeignKey(nameof(RuleId))]
    public int RuleId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? FixedValue { get; set; }
    public string? Comments { get; set; }

    [NotMapped]
    public Regex? Regex { get; set; }
}