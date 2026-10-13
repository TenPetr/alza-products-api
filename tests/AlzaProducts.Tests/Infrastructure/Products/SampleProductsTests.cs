using System.Data.SqlTypes;
using AlzaProducts.Infrastructure.Products;

namespace AlzaProducts.Tests.Infrastructure.Products;

public class SampleProductsTests
{
    [Fact]
    public void Create_ReturnsMoreThanTwoPagesOfProducts()
    {
        // The default page size is 10
        Assert.True(SampleProducts.Create().Count > 20);
    }

    [Fact]
    public void Create_HasUniqueIds()
    {
        var products = SampleProducts.Create();

        Assert.Equal(products.Count, products.Select(product => product.Id).Distinct().Count());
    }

    // Random GUIDs would land at random places in SQL Server's clustered index and fragment it;
    // SqlGuid compares the way SQL Server sorts, and new ids come from EF Core's SequentialGuidValueGenerator
    [Fact]
    public void Create_HasIdsInSqlServerSortOrder()
    {
        var ids = SampleProducts.Create().Select(product => new SqlGuid(product.Id)).ToList();

        Assert.Equal(ids.Order(), ids);
    }

    [Fact]
    public void Create_HasUniqueNames_SoOrderingIsDeterministic()
    {
        var products = SampleProducts.Create();

        Assert.Equal(
            products.Count,
            products.Select(product => product.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Create_IncludesProductsWithAndWithoutDescription()
    {
        var products = SampleProducts.Create();

        Assert.Contains(products, product => product.Description is null);
        Assert.Contains(products, product => product.Description is not null);
    }

    [Fact]
    public void Create_ReturnsNewInstancesOnEveryCall()
    {
        var first = SampleProducts.Create();
        var second = SampleProducts.Create();

        first[0].UpdateDescription("Changed");

        Assert.NotEqual("Changed", second[0].Description);
    }
}
