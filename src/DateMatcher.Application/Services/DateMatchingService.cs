using System.Globalization;
using DateMatcher.Application.DTOs;
using DateMatcher.Application.Interfaces;

namespace DateMatcher.Application.Services;

public class DateMatchingService : IDateMatchingService
{
    public DateMatchResponseDto FindMatches(DateMatchRequestDto request)
    {
        var matches = new List<string>();

        for (var year = request.StartYear; year <= request.EndYear; year++)
        {
            for (var month = 1; month <= 12; month++)
            {
                if (request.DayOfMonth > DateTime.DaysInMonth(year, month))
                {
                    continue;
                }

                var date = new DateTime(year, month, request.DayOfMonth);

                if (date.DayOfWeek != request.DayOfWeek)
                {
                    continue;
                }

                matches.Add(date.ToString("MMM-yyyy", CultureInfo.InvariantCulture));
            }
        }

        return new DateMatchResponseDto
        {
            Matches = matches
        };
    }
}
