using Core.Database.ServiceDbModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Core.Database
{
    /// <summary>
    /// Conexión con la base de datos, en el esquema del servicio
    /// </summary>
    public class ServiceDbContext : DbContext
    {
        /// <summary>
        /// Estado de los clientes
        /// </summary>
        public DbSet<Client> EstadoCliente { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().ToTable(b => b.IsMemoryOptimized());
            modelBuilder.HasDefaultSchema("service");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(SettingsService.Instance.SqlConnection);
        }
    }
}
