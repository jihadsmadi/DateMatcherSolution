using DateMatcher.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DateMatcher.Web.Pages;

public class IndexModel : PageModel
{
    public DateMatchRequestDto SearchRequest { get; private set; } = new()
    {
        StartYear = DateTime.Today.Year,
        EndYear = DateTime.Today.Year,
        DayOfMonth = 1
    };

    public SelectList DayOfWeekOptions { get; private set; } = null!;

    public void OnGet() => DayOfWeekOptions = BuildDayOfWeekOptions();

    private static SelectList BuildDayOfWeekOptions() =>
        new(Enum.GetValues<DayOfWeek>(), nameof(DayOfWeek));
}
