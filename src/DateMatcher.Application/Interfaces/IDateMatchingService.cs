using DateMatcher.Application.DTOs;

namespace DateMatcher.Application.Interfaces;

public interface IDateMatchingService
{
    DateMatchResponseDto FindMatches(DateMatchRequestDto request);
}
