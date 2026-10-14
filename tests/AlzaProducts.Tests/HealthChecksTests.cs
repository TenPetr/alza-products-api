using System.Net;
using AlzaProducts.Tests.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AlzaProducts.Tests;

[Collection(SqlServerCollection.Name)]
public sealed class HealthChecksTests(SqlServerFixture sqlServer)
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task WithMockData_IsHealthy(string path)
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.WithWebHostBuilder(builder => builder.UseSetting("DataSource", "Mock")).CreateClient();

        await AssertHealthAsync(client, path, HttpStatusCode.OK, "Healthy");
    }

    [SqlServerFact]
    public async Task Ready_WithReachableDatabase_IsHealthy()
    {
        var connectionString = await sqlServer.CreateDatabaseAsync();
        await using var application = new WebApplicationFactory<Program>();
        using var client = CreateDatabaseClient(application, connectionString, "Development");

        await AssertHealthAsync(client, "/health/ready", HttpStatusCode.OK, "Healthy");
    }

    // Production applies no migrations on startup, so the application starts even while the database is down
    [Fact]
    public async Task UnreachableDatabase_MakesTheApplicationNotReadyButStillLive()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = CreateDatabaseClient(
            application,
            "Server=localhost,1;Database=AlzaProducts;User Id=sa;Password=Unused_Passw0rd;TrustServerCertificate=True;Connect Timeout=2",
            "Production");

        await AssertHealthAsync(client, "/health/ready", HttpStatusCode.ServiceUnavailable, "Unhealthy");
        await AssertHealthAsync(client, "/health/live", HttpStatusCode.OK, "Healthy");
    }

    private static HttpClient CreateDatabaseClient(
        WebApplicationFactory<Program> application,
        string connectionString,
        string environment) =>
        application
            .WithWebHostBuilder(builder => builder
                .UseEnvironment(environment)
                .UseSetting("DataSource", "Database")
                .UseSetting("ConnectionStrings:Products", connectionString))
            .CreateClient();

    private static async Task AssertHealthAsync(HttpClient client, string path, HttpStatusCode status, string health)
    {
        var response = await client.GetAsync(path);

        Assert.Equal(status, response.StatusCode);
        Assert.Equal(health, await response.Content.ReadAsStringAsync());
    }
}
