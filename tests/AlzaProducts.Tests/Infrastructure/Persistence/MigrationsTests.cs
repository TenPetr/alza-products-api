using AlzaProducts.Application.Products;
using AlzaProducts.Domain;
using AlzaProducts.Infrastructure.Persistence;
using AlzaProducts.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;

namespace AlzaProducts.Tests.Infrastructure.Persistence;

[Collection(SqlServerCollection.Name)]
public sealed class MigrationsTests(SqlServerFixture sqlServer)
{
    // Compares the model with the last migration; needs no database, so it runs even without Docker
    [Fact]
    public void Model_HasNoChangesWithoutAMigration()
    {
        using var context = new AlzaProductsDbContextFactory().CreateDbContext([]);

        Assert.False(
            context.Database.HasPendingModelChanges(),
            "The EF Core model changed, add a migration: "
            + "dotnet ef migrations add <Name> --project src/AlzaProducts --output-dir Infrastructure/Persistence/Migrations");
    }

    [SqlServerFact]
    public async Task NewDatabase_IsSeededWithTheSampleProducts()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync();
        await using var context = SqlServerFixture.CreateContext(connectionString);

        var stored = await context.Products.ToListAsync();

        var expected = SampleProducts.Create().Select(ProductDto.FromProduct).OrderBy(product => product.Id);
        Assert.Equal(expected, stored.Select(ProductDto.FromProduct).OrderBy(product => product.Id));
    }

    // Seeding runs on every migration, so it must leave a database that already has products alone
    [SqlServerFact]
    public async Task Seeding_DoesNotTouchADatabaseThatHasProducts()
    {
        var product = new Product(Guid.NewGuid(), "Laptop", new Uri("https://cdn.example.com/products/laptop.jpg"), 999m);
        var connectionString = await sqlServer.CreateDatabaseAsync([product]);
        await using var context = SqlServerFixture.CreateContext(connectionString);

        await context.Database.MigrateAsync();

        var stored = Assert.Single(await context.Products.ToListAsync());
        Assert.Equal(product.Id, stored.Id);
    }
}
