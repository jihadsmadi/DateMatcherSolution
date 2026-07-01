namespace DateMatcher.Application.DTOs;

public class DateMatchResponseDto
{
    public IReadOnlyList<string> Matches { get; init; } = [];
}
