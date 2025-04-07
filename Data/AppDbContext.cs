using Microsoft.EntityFrameworkCore;
using ArbitrageApp.Models;

namespace ArbitrageApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ArbitrageOpportunity> ArbitrageOpportunities { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArbitrageOpportunity>()
                .HasIndex(a => a.Symbol);
        }
    }
}
