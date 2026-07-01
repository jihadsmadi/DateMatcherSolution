namespace DateMatcher.Domain.Entities;

public class SearchLog
{
    public int Id { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public int DayOfMonth { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string ResponseJson { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
}
