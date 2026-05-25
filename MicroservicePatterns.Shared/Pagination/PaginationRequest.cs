using System.ComponentModel;

namespace MicroservicePatterns.Shared.Pagination;

/// <summary>
/// Represents a pagination request with page size and page index.
/// </summary>
/// <param name="PageSize">Number of items per page (default: 20).</param>
/// <param name="PageIndex">Zero-based page index (default: 0).</param>
public record PaginationRequest(
    [property: DefaultValue(20)] int PageSize = 20,
    [property: DefaultValue(0)] int PageIndex = 0);
