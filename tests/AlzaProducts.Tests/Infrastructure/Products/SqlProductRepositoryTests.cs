using AlzaProducts.Application.Paging;
using AlzaProducts.Domain;
using AlzaProducts.Infrastructure.Products;
using AlzaProducts.Tests.Infrastructure.Persistence;

namespace AlzaProducts.Tests.Infrastructure.Products;

/// <summary>
/// Runs the repository against a real SQL Server, which checks what the mock data cannot:
/// the SQL that EF Core generates, the sorting done by the database and the column mapping
/// </summary>
[Collection(SqlServerCollection.Name)]
public sealed class SqlProductRepositoryTests(SqlServerFixture sqlServer)
{
    [SqlServerFact]
    public async Task GetAllAsync_ReturnsProductsInTheSameOrderAsTheMockData()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync(SampleProducts.Create());
        await using var context = SqlServerFixture.CreateContext(connectionString);

        var products = await new SqlProductRepository(context).GetAllAsync();

        var mockProducts = await new InMemoryProductRepository(SampleProducts.Create()).GetAllAsync();
        Assert.Equal(mockProducts.Select(product => product.Id), products.Select(product => product.Id));
    }

    [SqlServerFact]
    public async Task GetAllAsync_OrdersByNameIgnoringCase()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync(
            [CreateProduct("Mouse"), CreateProduct("keyboard"), CreateProduct("Laptop")]);
        await using var context = SqlServerFixture.CreateContext(connectionString);

        var products = await new SqlProductRepository(context).GetAllAsync();

        string[] expected = ["keyboard", "Laptop", "Mouse"];
        Assert.Equal(expected, products.Select(product => product.Name));
    }

    [SqlServerFact]
    public async Task GetPageAsync_ReturnsRequestedPageAndTotalCount()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync(SampleProducts.Create());
        await using var context = SqlServerFixture.CreateContext(connectionString);
        var repository = new SqlProductRepository(context);
        var all = await repository.GetAllAsync();

        var page = await repository.GetPageAsync(new PageRequest(2, 10));

        Assert.Equal(all.Skip(10).Take(10).Select(product => product.Id), page.Items.Select(product => product.Id));
        Assert.Equal(all.Count, page.TotalCount);
    }

    [SqlServerFact]
    public async Task GetPageAsync_WithHugePageNumber_ReturnsNoProducts()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync([CreateProduct("A"), CreateProduct("B")]);
        await using var context = SqlServerFixture.CreateContext(connectionString);

        var page = await new SqlProductRepository(context).GetPageAsync(new PageRequest(int.MaxValue, PageRequest.MaxPageSize));

        Assert.Empty(page.Items);
        Assert.Equal(2, page.TotalCount);
    }

    [SqlServerFact]
    public async Task GetByIdAsync_ReturnsProductWithAllItsValues()
    {
        var expected = new Product(
            Guid.NewGuid(),
            "HDMI Cable 2 m",
            new Uri("https://cdn.example.com/products/hdmi-cable-2-m.jpg"),
            249.90m,
            "Supports 4K at 120 Hz");
        var connectionString = await sqlServer.CreateDatabaseAsync([expected]);
        await using var context = SqlServerFixture.CreateContext(connectionString);

        var product = await new SqlProductRepository(context).GetByIdAsync(expected.Id);

        Assert.NotNull(product);
        Assert.Equal(expected.Name, product.Name);
        Assert.Equal(expected.ImgUri, product.ImgUri);
        Assert.Equal(249.90m, product.Price);
        Assert.Equal(expected.Description, product.Description);
    }

    [SqlServerFact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync([CreateProduct("A")]);
        await using var context = SqlServerFixture.CreateContext(connectionString);

        var product = await new SqlProductRepository(context).GetByIdAsync(Guid.NewGuid());

        Assert.Null(product);
    }

    [SqlServerFact]
    public async Task UpdateAsync_StoresChangesInTheDatabase()
    {
        var original = CreateProduct("Laptop", "Old description");
        var connectionString = await sqlServer.CreateDatabaseAsync([original]);
        await using (var context = SqlServerFixture.CreateContext(connectionString))
        {
            var repository = new SqlProductRepository(context);
            var product = await repository.GetByIdAsync(original.Id);
            Assert.NotNull(product);
            product.UpdateDescription("New description");

            await repository.UpdateAsync(product);
        }

        Assert.Equal("New description", await ReadDescriptionAsync(connectionString, original.Id));
    }

    [SqlServerFact]
    public async Task UpdateAsync_WithProductNotLoadedFromTheRepository_StoresChanges()
    {
        var original = CreateProduct("Laptop", "Old description");
        var connectionString = await sqlServer.CreateDatabaseAsync([original]);
        var changed = new Product(original.Id, original.Name, original.ImgUri, original.Price, "New description");

        await using (var context = SqlServerFixture.CreateContext(connectionString))
        {
            await new SqlProductRepository(context).UpdateAsync(changed);
        }

        Assert.Equal("New description", await ReadDescriptionAsync(connectionString, original.Id));
    }

    [SqlServerFact]
    public async Task ChangesToReturnedProduct_AreNotStoredWithoutUpdate()
    {
        var original = CreateProduct("Laptop", "Old description");
        var connectionString = await sqlServer.CreateDatabaseAsync([original]);
        await using (var context = SqlServerFixture.CreateContext(connectionString))
        {
            var product = await new SqlProductRepository(context).GetByIdAsync(original.Id);
            Assert.NotNull(product);
            product.UpdateDescription("Changed but not saved");
        }

        Assert.Equal("Old description", await ReadDescriptionAsync(connectionString, original.Id));
    }

    [SqlServerFact]
    public async Task UpdateAsync_WithUnknownProduct_Throws()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync([CreateProduct("A")]);
        await using var context = SqlServerFixture.CreateContext(connectionString);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new SqlProductRepository(context).UpdateAsync(CreateProduct("Unknown")));
    }

    // Reads through a new EF Core session, so the value really comes from the database
    private static async Task<string?> ReadDescriptionAsync(string connectionString, Guid id)
    {
        await using var context = SqlServerFixture.CreateContext(connectionString);
        var product = await context.Products.FindAsync(id);
        return product?.Description;
    }

    private static Product CreateProduct(string name, string? description = null) =>
        new(Guid.NewGuid(), name, new Uri("https://cdn.example.com/products/test.jpg"), 100m, description);
}
