using AlzaProducts.Application.Products;

namespace AlzaProducts.Application;

public static class DependencyInjection
{
    /// <summary>Registers the application use cases</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
