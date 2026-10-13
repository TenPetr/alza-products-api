namespace AlzaProducts.Infrastructure;

/// <summary>
/// Where the products come from, chosen by the "DataSource" setting
/// </summary>
public enum DataSource
{
    /// <summary>Sample products kept in memory; changes are lost when the application stops</summary>
    Mock,

    /// <summary>Products stored in SQL Server</summary>
    Database,
}
