using Core.Database.Model;
using Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Core.Database
{
    public class IdeDbContext : DbContext
    {
        /// <summary>
        /// Tabla con las reglas que se aplican en el proceso de cohesion
        /// </summary>
        public DbSet<Regla> Reglas { get; set; }

        /// <summary>
        /// Variables que debe mirar cada regla en el proceso de cohesion
        /// </summary>
        public DbSet<ReglaAtributo> ReglasAtributos { get; set; }

        /// <summary>
        /// Datos para completar las variables en la preview de etiquetas
        /// </summary>
        public DbSet<TestVar> EtiquetasDatosPrueba { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestVar>().ToTable(b => b.IsMemoryOptimized());
            modelBuilder.HasDefaultSchema("ide");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(SettingsService.Instance.SqlConnection);
        }
    }
}
