using Core.Database.IdeDbModels;
using Core.Services;

using Microsoft.EntityFrameworkCore;

namespace Core.Database;

/// <summary>
/// Conexión con la base de datos, en el esquema del editor
/// </summary>
public class IdeDbContext : DbContext
{
    /// <summary>
    /// Reglas que se aplican en el proceso de verificación
    /// </summary>
    public DbSet<Rule> Rule { get; set; }

    /// <summary>
    /// Variables que debe mirar cada regla en el proceso de verificación
    /// </summary>
    public DbSet<RuleAttributes> RuleAttributes { get; set; }

    /// <summary>
    /// Asociación entre las reglas y las etiquetas
    /// </summary>
    public DbSet<RuleLabel> RuleLabel { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ide");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(SettingsService.Instance.SqlConnection);
    }
}