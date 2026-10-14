using AlzaProducts.Application.Products;
using AlzaProducts.Infrastructure.Persistence;
using AlzaProducts.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;

namespace AlzaProducts.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the data source chosen by the "DataSource" setting: Mock (in-memory sample data)
    /// or Database (SQL Server, connection string "Products")
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        switch (GetDataSource(configuration))
        {
            case DataSource.Mock:
                // Singleton, so the in-memory data and its changes survive between requests
                services.AddSingleton<IProductRepository>(_ => new InMemoryProductRepository(SampleProducts.Create()));
                break;

            case DataSource.Database:
                var connectionString = configuration.GetConnectionString("Products");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Connection string Products is missing");
                }

                // Retries transient errors, e.g. when the database briefly drops the connection;
                // an empty database gets the sample products when it is migrated
                services.AddDbContext<AlzaProductsDbContext>(options => options
                    .UseSqlServer(connectionString, sqlServer => sqlServer.EnableRetryOnFailure())
                    .UseSampleProductsSeeding());
                    
                // Scoped, like the DbContext: every request gets its own
                services.AddScoped<IProductRepository, SqlProductRepository>();
                break;
        }

        return services;
    }

    private static DataSource GetDataSource(IConfiguration configuration)
    {
        var value = configuration[nameof(DataSource)];
        return Enum.TryParse<DataSource>(value, ignoreCase: true, out var dataSource) && Enum.IsDefined(dataSource)
            ? dataSource
            : throw new InvalidOperationException($"Setting DataSource must be Mock or Database, but it is '{value}'");
    }
}
