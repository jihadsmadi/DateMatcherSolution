using DateMatcher.Application.DTOs;
using DateMatcher.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DateMatcher.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DateMatcherController(IDateMatchingService dateMatchingService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(DateMatchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<DateMatchResponseDto> Search([FromBody] DateMatchRequestDto request) =>
        Ok(dateMatchingService.FindMatches(request));
}
