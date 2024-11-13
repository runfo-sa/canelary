using Microsoft.EntityFrameworkCore;
using VersionDatabase.Db.Models;
using VersionDatabase.Models;

namespace VersionDatabase.Db
{
    public class DatabaseDbContext : DbContext
    {
        public DbSet<Configuracion> Configuracion { get; set; }
        public DbSet<DefinicionEtiqueta> DefinicionEtiquetas { get; set; }
        public DbSet<Etiqueta> Etiquetas { get; set; }
        public DbSet<FormatoEtiqueta> FormatoEtiquetas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Settings.Instance.Schema);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Settings.Instance.SqlConnection);
        }
    }
}