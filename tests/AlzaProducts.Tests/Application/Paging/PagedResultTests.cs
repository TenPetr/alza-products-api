using AlzaProducts.Application.Paging;

namespace AlzaProducts.Tests.Application.Paging;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(20, 10, 2)]
    [InlineData(25, 10, 3)]
    public void TotalPages_CountsAlsoThePartlyFilledLastPage(int totalCount, int pageSize, int expected)
    {
        var result = new PagedResult<int>([], 1, pageSize, totalCount);

        Assert.Equal(expected, result.TotalPages);
    }

    [Fact]
    public void Map_ConvertsItemsAndKeepsPagingInformation()
    {
        var result = new PagedResult<int>([1, 2], 3, 2, 6);

        var mapped = result.Map(number => $"#{number}");

        string[] expected = ["#1", "#2"];
        Assert.Equal(expected, mapped.Items);
        Assert.Equal(3, mapped.Page);
        Assert.Equal(2, mapped.PageSize);
        Assert.Equal(6, mapped.TotalCount);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 10, -1)]
    public void Constructor_WithInvalidPagingInformation_Throws(int page, int pageSize, int totalCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PagedResult<int>([], page, pageSize, totalCount));
    }
}
