using System.Net;
using System.Net.Http.Json;
using AlzaProducts.Application.Paging;
using AlzaProducts.Application.Products;
using AlzaProducts.Infrastructure.Products;
using AlzaProducts.Tests.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AlzaProducts.Tests.Api.Products;

/// <summary>
/// The whole application with the DataSource setting switched to Database, running against a real SQL Server
/// </summary>
[Collection(SqlServerCollection.Name)]
public sealed class ProductsApiDatabaseTests(SqlServerFixture sqlServer)
{
    [SqlServerFact]
    public async Task GetPage_ReadsProductsFromTheDatabase()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync(SampleProducts.Create());
        await using var application = new WebApplicationFactory<Program>();
        using var client = CreateDatabaseClient(application, connectionString);

        var page = await client.GetFromJsonAsync<PagedResult<ProductDto>>("/api/v2/products");

        Assert.NotNull(page);
        var expectedNames = SampleProducts.Create()
            .Select(product => product.Name)
            .Order(StringComparer.OrdinalIgnoreCase)
            .Take(10);
        Assert.Equal(expectedNames, page.Items.Select(product => product.Name));
        Assert.Equal(SampleProducts.Create().Count, page.TotalCount);
    }

    [SqlServerFact]
    public async Task UpdateDescription_IsStoredInTheDatabase()
    {
        var id = SampleProducts.Create()[0].Id;
        var connectionString = await sqlServer.CreateDatabaseAsync(SampleProducts.Create());
        await using var application = new WebApplicationFactory<Program>();
        using var client = CreateDatabaseClient(application, connectionString);

        var response = await client.PatchAsJsonAsync($"/api/v1/products/{id}", new { description = "Stored in SQL Server" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var context = SqlServerFixture.CreateContext(connectionString);
        var stored = await context.Products.FindAsync(id);
        Assert.Equal("Stored in SQL Server", stored?.Description);
    }

    private static HttpClient CreateDatabaseClient(WebApplicationFactory<Program> application, string connectionString) =>
        application
            .WithWebHostBuilder(builder => builder
                .UseSetting("DataSource", "Database")
                .UseSetting("ConnectionStrings:Products", connectionString))
            .CreateClient();
}
