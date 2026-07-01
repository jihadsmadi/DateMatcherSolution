using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DateMatcher.Application.DTOs;
using DateMatcher.Application.Interfaces;
using DateMatcher.Application.Mappings;

namespace DateMatcher.Web.Middleware;

public class SearchRequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<SearchRequestLoggingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task InvokeAsync(HttpContext context, ISearchLogRepository searchLogRepository)
    {
        if (!IsSearchRequest(context))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalResponseBody = context.Response.Body;

        context.Request.EnableBuffering();
        var requestBody = await ReadRequestBodyAsync(context.Request);

        using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        Exception? capturedException = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            responseBuffer.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBuffer).ReadToEndAsync();
            responseBuffer.Seek(0, SeekOrigin.Begin);
            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            try
            {
                var searchLog = BuildSearchLog(
                    requestBody,
                    responseBody,
                    capturedException,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                await searchLogRepository.AddAsync(searchLog, context.RequestAborted);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to persist search log.");
            }
        }
    }

    private static bool IsSearchRequest(HttpContext context) =>
        HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.StartsWithSegments("/api/datematcher", StringComparison.OrdinalIgnoreCase);

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static Domain.Entities.SearchLog BuildSearchLog(
        string requestBody,
        string responseBody,
        Exception? capturedException,
        int statusCode,
        long executionTimeMs)
    {
        var success = capturedException is null && statusCode < 400;
        var errorMessage = capturedException?.Message;

        if (!success && string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = TryExtractErrorMessage(responseBody);
        }

        if (TryParseApiRequest(requestBody, out var request))
        {
            return request.ToSearchLog(responseBody, success, errorMessage, executionTimeMs);
        }

        return SearchLogMappings.ToUnparseableRequestLog(
            responseBody,
            errorMessage ?? "Invalid or missing request body.",
            executionTimeMs);
    }

    private static bool TryParseApiRequest(string requestBody, out DateMatchRequestDto request)
    {
        request = new DateMatchRequestDto();

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return false;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<DateMatchRequestDto>(requestBody, JsonOptions);
            if (parsed is null)
            {
                return false;
            }

            request = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? TryExtractErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("errors", out var errorsElement))
            {
                if (errorsElement.ValueKind == JsonValueKind.Array)
                {
                    var errors = errorsElement.EnumerateArray()
                        .Select(error => error.GetString())
                        .Where(error => !string.IsNullOrWhiteSpace(error));

                    return string.Join("; ", errors!);
                }

                if (errorsElement.ValueKind == JsonValueKind.Object)
                {
                    var errors = errorsElement.EnumerateObject()
                        .SelectMany(property => property.Value.EnumerateArray())
                        .Select(error => error.GetString())
                        .Where(error => !string.IsNullOrWhiteSpace(error));

                    var joined = string.Join("; ", errors!);
                    if (!string.IsNullOrWhiteSpace(joined))
                    {
                        return joined;
                    }
                }
            }

            if (root.TryGetProperty("detail", out var detailElement))
            {
                var detail = detailElement.GetString();
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    return detail;
                }
            }

            if (root.TryGetProperty("error", out var errorElement))
            {
                return errorElement.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
