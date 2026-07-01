using DateMatcher.Application.Common;
using DateMatcher.Application.DTOs;
using FluentValidation;

namespace DateMatcher.Application.Validators;

public class DateMatchRequestValidator : AbstractValidator<DateMatchRequestDto>
{
    public DateMatchRequestValidator()
    {
        RuleFor(x => x.StartYear)
            .InclusiveBetween(ValidationConstants.MinYear, ValidationConstants.MaxYear)
            .WithMessage($"Start year must be between {ValidationConstants.MinYear} and {ValidationConstants.MaxYear}.");

        RuleFor(x => x.EndYear)
            .InclusiveBetween(ValidationConstants.MinYear, ValidationConstants.MaxYear)
            .WithMessage($"End year must be between {ValidationConstants.MinYear} and {ValidationConstants.MaxYear}.");

        RuleFor(x => x)
            .Must(x => x.StartYear <= x.EndYear)
            .WithMessage("Start year must be less than or equal to end year.")
            .When(x => x.StartYear >= ValidationConstants.MinYear
                       && x.EndYear >= ValidationConstants.MinYear
                       && x.StartYear <= ValidationConstants.MaxYear
                       && x.EndYear <= ValidationConstants.MaxYear);

        RuleFor(x => x.DayOfMonth)
            .InclusiveBetween(ValidationConstants.MinDayOfMonth, ValidationConstants.MaxDayOfMonth)
            .WithMessage($"Day of month must be between {ValidationConstants.MinDayOfMonth} and {ValidationConstants.MaxDayOfMonth}.");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage("Day of week is required.");
    }
}
