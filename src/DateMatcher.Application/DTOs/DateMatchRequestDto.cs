using System.ComponentModel.DataAnnotations;

namespace DateMatcher.Application.DTOs;

public class DateMatchRequestDto
{
    [Display(Name = "Start year")]
    public int StartYear { get; set; }

    [Display(Name = "End year")]
    public int EndYear { get; set; }

    [Display(Name = "Day of month")]
    public int DayOfMonth { get; set; }

    [Display(Name = "Day of week")]
    public DayOfWeek DayOfWeek { get; set; }
}
