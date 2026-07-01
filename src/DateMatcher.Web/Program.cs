using System.Text.Json.Serialization;
using DateMatcher.Application;
using DateMatcher.Infrastructure;
using DateMatcher.Web;
using DateMatcher.Web.Infrastructure;
using DateMatcher.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebPresentation();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();

builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseExceptionHandler();

var urls = builder.Configuration["ASPNETCORE_URLS"]
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
    ?? string.Empty;
var usesHttps = urls.Contains("https://", StringComparison.OrdinalIgnoreCase);

if (!app.Environment.IsDevelopment() && usesHttps)
{
    app.UseHsts();
}

if (usesHttps)
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseMiddleware<SearchRequestLoggingMiddleware>();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
