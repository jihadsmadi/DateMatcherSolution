using DateMatcher.Application.Common;
using DateMatcher.Application.DTOs;
using DateMatcher.Domain.Entities;

namespace DateMatcher.Application.Interfaces;

public interface ISearchLogRepository
{
    Task AddAsync(SearchLog searchLog, CancellationToken cancellationToken = default);

    Task<SearchLogListItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<SearchLogListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        SearchLogSortDto? sort = null,
        CancellationToken cancellationToken = default);
}
