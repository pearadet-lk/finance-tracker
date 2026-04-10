using FinanceTracker.Application.Queries.Transactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int year = 0)
    {
        var targetYear = year == 0 ? DateTime.UtcNow.Year : year;
        var result = await _mediator.Send(new GetDashboardQuery(GetUserId(), targetYear));
        return Ok(result);
    }
}
