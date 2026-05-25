namespace Odin.DDD.Repositories;

/// <summary>
/// Represents pagination settings for a repository query.
/// </summary>
public sealed record Pagination
{
    /// <summary>
    /// Creates pagination settings from a one-based page number and page size.
    /// </summary>
    /// <param name="pageNumber">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of records in the page.</param>
    public Pagination(int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// The one-based page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// The maximum number of records in the page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// The number of records to skip.
    /// </summary>
    public int Skip => checked((PageNumber - 1) * PageSize);

    /// <summary>
    /// The maximum number of records to take.
    /// </summary>
    public int Take => PageSize;
}
