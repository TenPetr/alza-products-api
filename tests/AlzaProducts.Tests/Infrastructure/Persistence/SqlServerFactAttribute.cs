namespace AlzaProducts.Tests.Infrastructure.Persistence;

/// <summary>
/// A test that needs SQL Server, which runs in Docker. Without Docker it is skipped on a developer's machine,
/// but on CI it runs and fails, so a broken Docker can't turn into a green build with these tests silently skipped
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (!SqlServerFixture.IsDockerAvailable && !IsRunningOnCi)
        {
            Skip = "Docker is not running, so there is no SQL Server to test against";
        }
    }

    // GitHub Actions and most other CI systems set the CI environment variable, usually to "true"
    private static bool IsRunningOnCi =>
        Environment.GetEnvironmentVariable("CI") is { Length: > 0 } value
        && !value.Equals("false", StringComparison.OrdinalIgnoreCase);
}
