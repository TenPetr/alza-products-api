using AlzaProducts.Application.Paging;

namespace AlzaProducts.Tests.Application.Paging;

public class PageRequestTests
{
    [Fact]
    public void Constructor_WithoutArguments_RequestsFirstPageOfTenItems()
    {
        var request = new PageRequest();

        Assert.Equal(1, request.Page);
        Assert.Equal(10, request.PageSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(PageRequest.MaxPageSize)]
    public void Constructor_WithPageSizeWithinLimits_KeepsIt(int pageSize)
    {
        var request = new PageRequest(1, pageSize);

        Assert.Equal(pageSize, request.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithPageBelowOne_Throws(int page)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PageRequest(page));

        Assert.Equal("page", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(PageRequest.MaxPageSize + 1)]
    public void Constructor_WithPageSizeOutOfRange_Throws(int pageSize)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PageRequest(1, pageSize));

        Assert.Equal("pageSize", exception.ParamName);
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 7, 14)]
    public void Skip_IsNumberOfItemsOnPreviousPages(int page, int pageSize, int expected)
    {
        var request = new PageRequest(page, pageSize);

        Assert.Equal(expected, request.Skip);
    }

    [Fact]
    public void Skip_WithHugePageNumber_IsCappedInsteadOfOverflowing()
    {
        var request = new PageRequest(int.MaxValue, PageRequest.MaxPageSize);

        Assert.Equal(int.MaxValue, request.Skip);
    }
}
