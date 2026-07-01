namespace DateMatcher.Infrastructure.Options;

public class DatabaseOptions
{
    public const string SectionName = "DatabaseOptions";

    public string DefaultConnection { get; set; } = string.Empty;
}
