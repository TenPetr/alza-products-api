namespace AlzaProducts.Application.Paging;

/// <summary>
/// Which page of a list to return, pages are numbered from 1
/// </summary>
public sealed record PageRequest
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public PageRequest(int page = 1, int pageSize = DefaultPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, MaxPageSize);

        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }

    public int PageSize { get; }

    /// <summary>
    /// Number of items on the pages before this one; capped at int.MaxValue instead of overflowing,
    /// so a huge page number simply returns no items
    /// </summary>
    public int Skip => (int)Math.Min((long)(Page - 1) * PageSize, int.MaxValue);
}
