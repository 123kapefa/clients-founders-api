namespace Teledok.Shared.SharedKernel;

public sealed record PageResult<T>
{
    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    internal PageResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}

public static class PageResult
{
    public const int DEFAULT_PAGE = 1;

    public const int DEFAULT_PAGE_SIZE = 20;

    public const int MAX_PAGE_SIZE = 100;

    public static PageResult<T> Create<T>(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        return new PageResult<T>(items, page, pageSize, totalCount);
    }

    public static PageResult<T> Empty<T>(int page, int pageSize) => Create<T>([], page, pageSize, 0);
}
