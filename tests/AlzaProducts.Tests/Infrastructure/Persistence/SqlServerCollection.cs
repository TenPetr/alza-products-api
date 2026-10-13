namespace AlzaProducts.Tests.Infrastructure.Persistence;

/// <summary>
/// Test classes in this collection share one SQL Server container
/// </summary>
[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
