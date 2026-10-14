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
    // The same pinned image as compose.yaml, so it is usually downloaded already
    private const string Image = "mcr.microsoft.com/mssql/server:2022-CU27-ubuntu-22.04";

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
    /// Creates a new database the way the application does, through the migrations and the seeding, so it starts
    /// with the sample products; given products replace them. Every test gets its own database, so tests never share
    /// data. Returns its connection string
    /// </summary>
    public async Task<string> CreateDatabaseAsync(IEnumerable<Product>? products = null)
    {
        var connectionString = NewDatabaseConnectionString();

        await using var context = CreateContext(connectionString);
        await context.Database.MigrateAsync();
        if (products is not null)
        {
            await context.Products.ExecuteDeleteAsync();
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        return connectionString;
    }

    /// <summary>Connection string of a new database that does not exist yet</summary>
    public string NewDatabaseConnectionString()
    {
        var container = _container ?? throw new InvalidOperationException("SQL Server is not running, Docker is not available");
        return new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = $"AlzaProducts_{Guid.NewGuid():N}",
        }.ConnectionString;
    }

    // Configured like the application, seeding included
    public static AlzaProductsDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<AlzaProductsDbContext>()
            .UseSqlServer(connectionString)
            .UseSampleProductsSeeding()
            .Options);
}
