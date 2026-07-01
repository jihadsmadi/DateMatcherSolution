namespace DateMatcher.Application.DTOs;

public class SearchLogSortDto
{
    public string SortBy { get; set; } = "createdAt";

    public bool Descending { get; set; } = true;
}
