using Dica80.CleanArchitecture.Application.DTOs;

namespace Dica80.CleanArchitecture.Application.Common;

/// <summary>
/// Pagination parameters for queries
/// </summary>
public class PaginationParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Paginated result wrapper
/// </summary>
/// <typeparam name="T">Type of items</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
