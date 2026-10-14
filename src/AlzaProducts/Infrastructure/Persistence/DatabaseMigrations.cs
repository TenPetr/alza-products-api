using Microsoft.EntityFrameworkCore;

namespace AlzaProducts.Infrastructure.Persistence;

public static class DatabaseMigrations
{
    /// <summary>
    /// Creates the database or brings its schema up to date, then seeds it when it is empty (UseSampleProductsSeeding),
    /// when the products are stored in a database; does nothing for the mock data
    /// </summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        if (scope.ServiceProvider.GetService<AlzaProductsDbContext>() is not { } context)
        {
            return;
        }

        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new InvalidOperationException(
                "Cannot create or update the database: check that SQL Server is running and that the connection string "
                + "Products is right, or set DataSource to Mock to run without a database",
                exception);
        }
    }
}
