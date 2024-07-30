using Core.Services;
using Microsoft.EntityFrameworkCore;
using TwinsBackend.Database.Model;

namespace TwinsBackend.Database
{
    public class TwinsDbContext : DbContext
    {
        public DbSet<Variable> Variables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Twins");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(SettingsService.Instance.SqlConnection);
        }
    }
}
