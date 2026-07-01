using System.Text.Json;
using DateMatcher.Application.DTOs;
using DateMatcher.Domain.Entities;

namespace DateMatcher.Application.Mappings;

public static class SearchLogListMappings
{
    public static SearchLogListItemDto ToListItem(this SearchLog log) =>
        ToListItem(log, includeResponse: false);

    public static SearchLogListItemDto ToDetailItem(this SearchLog log) =>
        ToListItem(log, includeResponse: true);

    private static SearchLogListItemDto ToListItem(SearchLog log, bool includeResponse)
    {
        return new SearchLogListItemDto
        {
            Id = log.Id,
            CreatedAt = log.CreatedAt,
            StartYear = log.StartYear,
            EndYear = log.EndYear,
            DayOfMonth = log.DayOfMonth,
            DayOfWeek = log.DayOfWeek,
            Success = log.Success,
            ExecutionTimeMs = log.ExecutionTimeMs,
            MatchCount = TryGetMatchCount(log.ResponseJson),
            ErrorMessage = Truncate(log.ErrorMessage, 120),
            ResponseJson = includeResponse ? FormatResponseJson(log.ResponseJson) : string.Empty
        };
    }

    private static string FormatResponseJson(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return "[]";
        }

        try
        {
            using var document = JsonDocument.Parse(responseJson);
            return JsonSerializer.Serialize(
                document,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return responseJson;
        }
    }

    private static int? TryGetMatchCount(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseJson);
            var root = document.RootElement;

            if (root.TryGetProperty("matches", out var matchesElement)
                && matchesElement.ValueKind == JsonValueKind.Array)
            {
                return matchesElement.GetArrayLength();
            }

            if (root.TryGetProperty("Matches", out var pascalMatchesElement)
                && pascalMatchesElement.ValueKind == JsonValueKind.Array)
            {
                return pascalMatchesElement.GetArrayLength();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..(maxLength - 1)] + "…";
    }
}
