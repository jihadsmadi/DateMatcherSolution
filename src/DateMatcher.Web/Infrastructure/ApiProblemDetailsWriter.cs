using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace DateMatcher.Web.Infrastructure;

public static class ApiProblemDetailsWriter
{
    private const string ValidationType = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
    private const string ServerErrorType = "https://tools.ietf.org/html/rfc9110#section-15.6.1";

    public static ProblemDetails CreateValidationProblemDetails(
        IDictionary<string, string[]> errors,
        string? detail = null)
    {
        return new ValidationProblemDetails(errors)
        {
            Type = ValidationType,
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Detail = detail
        };
    }

    public static ProblemDetails CreateServerErrorProblemDetails(string detail)
    {
        return new ProblemDetails
        {
            Type = ServerErrorType,
            Title = "An error occurred while processing your request.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = detail
        };
    }

    public static async Task WriteAsync(HttpContext context, ProblemDetails problemDetails)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
