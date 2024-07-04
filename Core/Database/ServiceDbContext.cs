using Core.Database.ServiceDbModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Core.Database
{
    /// <summary>
    /// Instancia de conexión con la base de datos del servicio
    /// </summary>
    public class ServiceDbContext : DbContext
    {
        /// <summary>
        /// Tabla que almacena el estado de todos los clientes
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
