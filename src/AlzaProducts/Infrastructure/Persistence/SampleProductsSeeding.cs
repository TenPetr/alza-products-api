using AlzaProducts.Domain;
using AlzaProducts.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;

namespace AlzaProducts.Infrastructure.Persistence;

public static class SampleProductsSeeding
{
    /// <summary>
    /// Fills an empty database with the sample products whenever EF Core migrates it; a database that already holds
    /// products is never touched, so seeding can't overwrite or duplicate live data
    /// </summary>
    /// <remarks>
    /// EF Core runs UseAsyncSeeding from MigrateAsync and UseSeeding from Migrate and the dotnet ef tools,
    /// so both are configured, as its documentation recommends
    /// </remarks>
    public static TBuilder UseSampleProductsSeeding<TBuilder>(this TBuilder options)
        where TBuilder : DbContextOptionsBuilder
    {
        options
            .UseSeeding((context, _) =>
            {
                var products = context.Set<Product>();
                if (!products.Any())
                {
                    products.AddRange(SampleProducts.Create());
                    context.SaveChanges();
                }
            })
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                var products = context.Set<Product>();
                if (!await products.AnyAsync(cancellationToken))
                {
                    products.AddRange(SampleProducts.Create());
                    await context.SaveChangesAsync(cancellationToken);
                }
            });
        return options;
    }
}
