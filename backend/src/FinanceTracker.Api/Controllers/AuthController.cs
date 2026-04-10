using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(IAppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _context.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { error = "Email already registered" });

        var user = new User
        {
            Email = req.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FirstName = req.FirstName,
            LastName = req.LastName
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(BuildAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid credentials" });

        return Ok(BuildAuthResponse(user));
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds);

        return new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            Guid.NewGuid().ToString(), // Refresh token placeholder
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName));
    }
}
