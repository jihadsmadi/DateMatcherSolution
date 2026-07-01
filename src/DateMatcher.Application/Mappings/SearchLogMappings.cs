using DateMatcher.Application.DTOs;
using DateMatcher.Domain.Entities;

namespace DateMatcher.Application.Mappings;

public static class SearchLogMappings
{
    public static SearchLog ToSearchLog(
        this DateMatchRequestDto request,
        string responseJson,
        bool success,
        string? errorMessage,
        long executionTimeMs)
    {
        return new SearchLog
        {
            StartYear = request.StartYear,
            EndYear = request.EndYear,
            DayOfMonth = request.DayOfMonth,
            DayOfWeek = request.DayOfWeek,
            ResponseJson = NormalizeResponseJson(responseJson),
            Success = success,
            ErrorMessage = errorMessage,
            ExecutionTimeMs = executionTimeMs,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static SearchLog ToUnparseableRequestLog(
        string responseJson,
        string? errorMessage,
        long executionTimeMs) =>
        new()
        {
            ResponseJson = NormalizeResponseJson(responseJson),
            Success = false,
            ErrorMessage = errorMessage ?? "Invalid or missing request body.",
            ExecutionTimeMs = executionTimeMs,
            CreatedAt = DateTime.UtcNow
        };

    private static string NormalizeResponseJson(string responseJson) =>
        string.IsNullOrWhiteSpace(responseJson) ? "{}" : responseJson;
}
