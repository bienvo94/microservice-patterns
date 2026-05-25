namespace MicroservicePatterns.Shared.Pagination;

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="TEntity">The type of items in the result set.</typeparam>
/// <param name="PageIndex">The current page index.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Count">The total number of items across all pages.</param>
/// <param name="Items">The items on the current page.</param>
public record PaginatedResult<TEntity>(int PageIndex, int PageSize, long Count, IEnumerable<TEntity> Items) where TEntity : class;