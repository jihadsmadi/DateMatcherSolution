using DateMatcher.Application.Interfaces;
using DateMatcher.Application.Services;
using DateMatcher.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DateMatcher.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<DateMatchRequestValidator>();
        services.AddMemoryCache(options => options.SizeLimit = 500);
        services.AddScoped<DateMatchingService>();
        services.AddScoped<IDateMatchingService, CachedDateMatchingService>();
        return services;
    }
}
