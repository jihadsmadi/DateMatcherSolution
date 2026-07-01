using DateMatcher.Application.DTOs;
using DateMatcher.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DateMatcher.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchLogsController(ISearchLogRepository searchLogRepository) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SearchLogListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SearchLogListItemDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var log = await searchLogRepository.GetByIdAsync(id, cancellationToken);
        return log is null ? NotFound() : Ok(log);
    }
}
