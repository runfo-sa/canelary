using Core.Database.IdeDbModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Core.Database
{
    public class IdeDbContext : DbContext
    {
        /// <summary>
        /// Tabla con las reglas que se aplican en el proceso de cohesion
        /// </summary>
        public DbSet<Rule> Rule { get; set; }

        /// <summary>
        /// Variables que debe mirar cada regla en el proceso de cohesion
        /// </summary>
        public DbSet<RuleAttributes> RuleAttributes { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DbSet<RuleLabel> RuleLabel { get; set; }

        /// <summary>
        /// Datos para completar las variables en la preview de etiquetas
        /// </summary>
        public DbSet<DummyDataModel> DummyData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("ide");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(SettingsService.Instance.SqlConnection);
        }
    }
}
