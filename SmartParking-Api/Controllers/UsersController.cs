using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartParking_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly SmartParkingDbContext _db;

    public UsersController(SmartParkingDbContext db) => _db = db;

    public record UpdateUserRoleRequest(string Email, string Role);

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // O JwtBearer extrai a identidade a partir das claims do token. :contentReference[oaicite:1]{index=1}
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr)) return Unauthorized();

        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return Unauthorized();

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.Role
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("role")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateUserRoleRequest req)
    {
        var email = req.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email em falta.");

        var roleNormalized = req.Role?.Trim();
        if (string.Equals(roleNormalized, "admin", System.StringComparison.OrdinalIgnoreCase))
            roleNormalized = "Admin";
        else if (string.Equals(roleNormalized, "user", System.StringComparison.OrdinalIgnoreCase))
            roleNormalized = "User";
        else
            return BadRequest("Role inválida. Use: Admin ou User.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return NotFound();

        user.Role = roleNormalized;
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.Email, user.Role });
    }
}
