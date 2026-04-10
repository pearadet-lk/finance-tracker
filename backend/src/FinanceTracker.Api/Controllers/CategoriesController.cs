using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IAppDbContext _context;

    public CategoriesController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type = null)
    {
        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(c => c.Type.ToString() == type);

        var result = await query
            .Select(c => new CategoryDto(c.Id, c.Name, c.Type.ToString(), c.Icon, c.Color))
            .ToListAsync();

        return Ok(result);
    }
}
