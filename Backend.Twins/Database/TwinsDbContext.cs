using Core.Services;
using Microsoft.EntityFrameworkCore;
using BackendTwins.Database.Model;

namespace BackendTwins.Database
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
