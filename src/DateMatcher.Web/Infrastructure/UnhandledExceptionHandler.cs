using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DateMatcher.Web.Infrastructure;

public class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred.");

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        if (IsApiRequest(httpContext))
        {
            var problemDetails = ApiProblemDetailsWriter.CreateServerErrorProblemDetails(
                "An unexpected error occurred.");

            await ApiProblemDetailsWriter.WriteAsync(httpContext, problemDetails);
            return true;
        }

        httpContext.Response.Redirect("/Error");
        return true;
    }

    private static bool IsApiRequest(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}
