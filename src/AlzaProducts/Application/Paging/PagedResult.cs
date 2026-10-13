namespace AlzaProducts.Application.Paging;

/// <summary>
/// One page of a list, with the information needed to ask for the other pages
/// </summary>
public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>Number of this page, starting at 1</summary>
    public int Page { get; }

    /// <summary>Maximum number of items on a page</summary>
    public int PageSize { get; }

    /// <summary>Number of items on all pages together</summary>
    public int TotalCount { get; }

    /// <summary>Number of pages, 0 when there are no items</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Items on this page</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Converts the items, keeping the paging information</summary>
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> map) =>
        new(Items.Select(map).ToList(), Page, PageSize, TotalCount);
}
