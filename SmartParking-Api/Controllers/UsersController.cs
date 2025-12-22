using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}