using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace DateMatcher.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebPresentation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
