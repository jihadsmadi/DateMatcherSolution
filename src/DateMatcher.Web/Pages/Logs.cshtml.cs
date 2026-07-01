using DateMatcher.Application.Common;
using DateMatcher.Application.DTOs;
using DateMatcher.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DateMatcher.Web.Pages;

public class LogsModel(ISearchLogRepository searchLogRepository) : PageModel
{
    private const int DefaultPageSize = 10;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "createdAt";

    [BindProperty(SupportsGet = true)]
    public bool SortDescending { get; set; } = true;

    public PagedResult<SearchLogListItemDto> Logs { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Logs = await searchLogRepository.GetPagedAsync(
            PageNumber,
            DefaultPageSize,
            new SearchLogSortDto { SortBy = SortBy, Descending = SortDescending },
            cancellationToken);

        if (PageNumber > 1 && Logs.Items.Count == 0 && Logs.TotalCount > 0)
        {
            return RedirectToPage(LogsSortHelper.GetRouteValues(Logs.TotalPages, SortBy, SortDescending));
        }

        return Page();
    }

    public Dictionary<string, string> GetRouteValues(int page) =>
        LogsSortHelper.GetRouteValues(page, SortBy, SortDescending);

    public Dictionary<string, string> GetSortRouteValues(string column) =>
        LogsSortHelper.GetSortRouteValues(column, SortBy, SortDescending);

    public string GetSortIndicator(string column) =>
        LogsSortHelper.GetSortIndicator(column, SortBy, SortDescending);

    public string GetSortAriaSort(string column) =>
        LogsSortHelper.GetSortAriaSort(column, SortBy, SortDescending);
}
