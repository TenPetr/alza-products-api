using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlzaProducts.Infrastructure.Persistence;

/// <summary>
/// Creates the DbContext for the EF Core tools (dotnet ef migrations add ...) without starting the application,
/// so creating a migration does not depend on the DataSource setting or a running database
/// </summary>
public sealed class AlzaProductsDbContextFactory : IDesignTimeDbContextFactory<AlzaProductsDbContext>
{
    // Creating migrations and SQL scripts needs the SQL Server provider but never connects to the database
    public AlzaProductsDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AlzaProductsDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AlzaProducts")
            .UseSampleProductsSeeding()
            .Options);
}
