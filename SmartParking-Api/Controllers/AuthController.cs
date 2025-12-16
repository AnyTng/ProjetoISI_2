using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Auth;
using SmartParking_Api.Data;
using SmartParking_Api.Models;

namespace SmartParking_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SmartParkingDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly JwtTokenService _jwt;

    public AuthController(SmartParkingDbContext db, IPasswordHasher<AppUser> hasher, JwtTokenService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    public record RegisterRequest(string UserName, string Email, string Password, string? Nome);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest("Email já existe.");

        if (await _db.Users.AnyAsync(u => u.UserName == req.UserName))
            return BadRequest("UserName já existe.");

        var user = new AppUser
        {
            UserName = req.UserName,
            Email = req.Email,
            Role = "User"
        };

        user.PasswordHash = _hasher.HashPassword(user, req.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.UserName, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
        if (user is null) return Unauthorized("Credenciais inválidas.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Credenciais inválidas.");

        // Se o hasher disser “rehash needed”, atualiza e guarda
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, req.Password);
            await _db.SaveChangesAsync();
        }

        var token = _jwt.CreateToken(user);
        return Ok(new { token });
    }
}
