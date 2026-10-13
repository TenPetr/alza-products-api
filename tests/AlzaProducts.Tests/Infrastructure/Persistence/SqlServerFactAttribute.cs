namespace AlzaProducts.Tests.Infrastructure.Persistence;

/// <summary>
/// A test that needs SQL Server, which runs in Docker; it is skipped, not failed, when Docker is not running
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (!SqlServerFixture.IsDockerAvailable)
        {
            Skip = "Docker is not running, so there is no SQL Server to test against";
        }
    }
}
