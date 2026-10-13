using AlzaProducts.Domain;
using AlzaProducts.Infrastructure.Persistence;
using DotNet.Testcontainers.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AlzaProducts.Tests.Infrastructure.Persistence;

/// <summary>
/// A real SQL Server in a Docker container (Testcontainers), started once for all tests that need it
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    // The image of the local development database, so it is usually downloaded already
    private const string Image = "mcr.microsoft.com/mssql/server:2022-latest";

    private MsSqlContainer? _container;

    /// <summary>Whether Docker answers; checked once per test run</summary>
    public static bool IsDockerAvailable { get; } = TestcontainersSettings.OS.DockerEndpointAuthConfig is not null;

    public async Task InitializeAsync()
    {
        if (!IsDockerAvailable)
        {
            return;
        }

        _container = new MsSqlBuilder(Image).Build();
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates a new database holding the given products, so tests never share data, and returns its connection string
    /// </summary>
    public async Task<string> CreateDatabaseAsync(IEnumerable<Product> products)
    {
        var container = _container ?? throw new InvalidOperationException("SQL Server is not running, Docker is not available");
        var connectionString = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = $"AlzaProducts_{Guid.NewGuid():N}",
        }.ConnectionString;

        await using var context = CreateContext(connectionString);
        await context.Database.EnsureCreatedAsync();
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        return connectionString;
    }

    public static AlzaProductsDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<AlzaProductsDbContext>().UseSqlServer(connectionString).Options);
}
