using AlzaProducts.Application.Products;
using AlzaProducts.Infrastructure.Products;

namespace AlzaProducts.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the data access, currently the in-memory mock data</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Singleton, so the in-memory data and its changes survive between requests
        services.AddSingleton<IProductRepository>(_ => new InMemoryProductRepository(SampleProducts.Create()));
        return services;
    }
}
