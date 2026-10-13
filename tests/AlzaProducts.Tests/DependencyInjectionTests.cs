using AlzaProducts.Application;
using AlzaProducts.Application.Products;
using AlzaProducts.Infrastructure;
using AlzaProducts.Infrastructure.Products;
using Microsoft.Extensions.DependencyInjection;

namespace AlzaProducts.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void ApplicationAndInfrastructure_RegisterResolvableServices()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<ProductService>(scope.ServiceProvider.GetRequiredService<IProductService>());
        Assert.IsType<InMemoryProductRepository>(scope.ServiceProvider.GetRequiredService<IProductRepository>());
    }

    [Fact]
    public void InMemoryRepository_IsSharedAcrossRequests()
    {
        using var provider = BuildServiceProvider();
        using var firstRequest = provider.CreateScope();
        using var secondRequest = provider.CreateScope();

        Assert.Same(
            firstRequest.ServiceProvider.GetRequiredService<IProductRepository>(),
            secondRequest.ServiceProvider.GetRequiredService<IProductRepository>());
    }

    // Same checks ASP.NET Core runs at startup in Development: every registration can be built
    private static ServiceProvider BuildServiceProvider() =>
        new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddInfrastructure()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
}
