using AlzaProducts.Application;
using AlzaProducts.Application.Products;
using AlzaProducts.Infrastructure;
using AlzaProducts.Infrastructure.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlzaProducts.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void MockDataSource_RegistersInMemoryRepository()
    {
        using var provider = BuildServiceProvider(("DataSource", "Mock"));
        using var scope = provider.CreateScope();

        Assert.IsType<ProductService>(scope.ServiceProvider.GetRequiredService<IProductService>());
        Assert.IsType<InMemoryProductRepository>(scope.ServiceProvider.GetRequiredService<IProductRepository>());
    }

    [Fact]
    public void MockDataSource_SharesRepositoryAcrossRequests()
    {
        using var provider = BuildServiceProvider(("DataSource", "Mock"));
        using var firstRequest = provider.CreateScope();
        using var secondRequest = provider.CreateScope();

        Assert.Same(
            firstRequest.ServiceProvider.GetRequiredService<IProductRepository>(),
            secondRequest.ServiceProvider.GetRequiredService<IProductRepository>());
    }

    [Fact]
    public void DatabaseDataSource_RegistersSqlRepositoryPerRequest()
    {
        using var provider = BuildServiceProvider(
            ("DataSource", "Database"),
            ("ConnectionStrings:Products", "Server=localhost;Database=AlzaProducts"));
        using var firstRequest = provider.CreateScope();
        using var secondRequest = provider.CreateScope();

        var repository = firstRequest.ServiceProvider.GetRequiredService<IProductRepository>();

        Assert.IsType<SqlProductRepository>(repository);
        Assert.NotSame(repository, secondRequest.ServiceProvider.GetRequiredService<IProductRepository>());
    }

    [Fact]
    public void DatabaseDataSource_WithoutConnectionString_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BuildServiceProvider(("DataSource", "Database")));

        Assert.Contains("Connection string Products", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Excel")]
    public void UnknownOrMissingDataSource_Throws(string? dataSource)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => BuildServiceProvider(("DataSource", dataSource)));

        Assert.Contains("DataSource must be Mock or Database", exception.Message);
    }

    // Same checks ASP.NET Core runs at startup in Development: every registration can be built
    private static ServiceProvider BuildServiceProvider(params (string Key, string? Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(setting => KeyValuePair.Create(setting.Key, setting.Value)))
            .Build();

        return new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddInfrastructure(configuration)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}
