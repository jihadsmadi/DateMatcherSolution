using DateMatcher.Application.Common;
using DateMatcher.Application.DTOs;
using DateMatcher.Application.Interfaces;
using DateMatcher.Application.Mappings;
using DateMatcher.Domain.Entities;
using DateMatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DateMatcher.Infrastructure.Persistence.Repositories;

public class SearchLogRepository(AppDbContext dbContext) : ISearchLogRepository
{
    public async Task AddAsync(SearchLog searchLog, CancellationToken cancellationToken = default)
    {
        dbContext.SearchLogs.Add(searchLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SearchLogListItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var log = await dbContext.SearchLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);

        return log?.ToDetailItem();
    }

    public async Task<PagedResult<SearchLogListItemDto>> GetPagedAsync(
        int page,
        int pageSize,
        SearchLogSortDto? sort = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var sortOptions = sort ?? new SearchLogSortDto();

        var query = ApplySort(dbContext.SearchLogs.AsNoTracking(), sortOptions);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SearchLogListItemDto>
        {
            Items = items.Select(log => log.ToListItem()).ToList(),
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<SearchLog> ApplySort(IQueryable<SearchLog> query, SearchLogSortDto sort)
    {
        var descending = sort.Descending;

        return sort.SortBy.ToLowerInvariant() switch
        {
            "id" => descending
                ? query.OrderByDescending(log => log.Id)
                : query.OrderBy(log => log.Id),
            "criteria" => descending
                ? query.OrderByDescending(log => log.StartYear).ThenByDescending(log => log.EndYear)
                : query.OrderBy(log => log.StartYear).ThenBy(log => log.EndYear),
            "status" => descending
                ? query.OrderByDescending(log => log.Success).ThenByDescending(log => log.CreatedAt)
                : query.OrderBy(log => log.Success).ThenBy(log => log.CreatedAt),
            "duration" => descending
                ? query.OrderByDescending(log => log.ExecutionTimeMs)
                : query.OrderBy(log => log.ExecutionTimeMs),
            _ => descending
                ? query.OrderByDescending(log => log.CreatedAt)
                : query.OrderBy(log => log.CreatedAt)
        };
    }
}
