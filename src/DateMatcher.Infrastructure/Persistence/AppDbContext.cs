using DateMatcher.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DateMatcher.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<SearchLog> SearchLogs => Set<SearchLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchLog>(entity =>
        {
            entity.ToTable("SearchLogs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.ErrorMessage).HasMaxLength(2000);
        });
    }
}
