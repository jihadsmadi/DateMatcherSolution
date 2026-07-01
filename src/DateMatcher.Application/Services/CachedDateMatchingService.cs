using DateMatcher.Application.DTOs;
using DateMatcher.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DateMatcher.Application.Services;

public class CachedDateMatchingService(
    IMemoryCache memoryCache,
    DateMatchingService inner) : IDateMatchingService
{
    private static readonly MemoryCacheEntryOptions CacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
        Size = 1
    };

    public DateMatchResponseDto FindMatches(DateMatchRequestDto request) =>
        memoryCache.GetOrCreate(
            BuildCacheKey(request),
            entry =>
            {
                entry.SetOptions(CacheEntryOptions);
                return inner.FindMatches(request);
            })!;

    private static string BuildCacheKey(DateMatchRequestDto request) =>
        $"matches:{request.StartYear}:{request.EndYear}:{request.DayOfMonth}:{request.DayOfWeek}";
}
