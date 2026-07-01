namespace DateMatcher.Web.Pages;

public static class LogsSortHelper
{
    public static Dictionary<string, string> GetRouteValues(int page, string sortBy, bool sortDescending) =>
        new()
        {
            ["PageNumber"] = page.ToString(),
            ["SortBy"] = sortBy,
            ["SortDescending"] = sortDescending.ToString().ToLowerInvariant()
        };

    public static Dictionary<string, string> GetSortRouteValues(string column, string sortBy, bool sortDescending)
    {
        var isActive = string.Equals(sortBy, column, StringComparison.OrdinalIgnoreCase);
        var nextDescending = isActive ? !sortDescending : column.Equals("createdAt", StringComparison.OrdinalIgnoreCase);

        return GetRouteValues(1, column, nextDescending);
    }

    public static string GetSortIndicator(string column, string sortBy, bool sortDescending)
    {
        if (!string.Equals(sortBy, column, StringComparison.OrdinalIgnoreCase))
        {
            return "↕";
        }

        return sortDescending ? "↓" : "↑";
    }

    public static string GetSortAriaSort(string column, string sortBy, bool sortDescending)
    {
        if (!string.Equals(sortBy, column, StringComparison.OrdinalIgnoreCase))
        {
            return "none";
        }

        return sortDescending ? "descending" : "ascending";
    }
}
