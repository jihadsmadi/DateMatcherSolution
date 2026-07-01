namespace DateMatcher.Application.DTOs;

public class SearchLogListItemDto
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public int StartYear { get; init; }
    public int EndYear { get; init; }
    public int DayOfMonth { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public bool Success { get; init; }
    public long ExecutionTimeMs { get; init; }
    public int? MatchCount { get; init; }
    public string? ErrorMessage { get; init; }
    public string ResponseJson { get; init; } = string.Empty;
}
