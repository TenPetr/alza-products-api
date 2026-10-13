using AlzaProducts.Domain;
using Microsoft.EntityFrameworkCore;

namespace AlzaProducts.Infrastructure.Persistence;

/// <summary>
/// EF Core session with the products database
/// </summary>
public sealed class AlzaProductsDbContext(DbContextOptions<AlzaProductsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    // Applies the mapping classes of this assembly, such as ProductConfiguration
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlzaProductsDbContext).Assembly);
}
