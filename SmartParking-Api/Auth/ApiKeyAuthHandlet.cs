using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartParking_Api.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartParking_Api.Auth;

public class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly SmartParkingDbContext _db;

    public ApiKeyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        SmartParkingDbContext db) : base(options, logger, encoder)
    {
        _db = db;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (HttpMethods.IsOptions(Request.Method))
            return AuthenticateResult.NoResult();

        if (!Request.Headers.TryGetValue(ApiKeyDefaults.HeaderName, out var provided))
            return AuthenticateResult.Fail("API key em falta.");

        var key = provided.ToString().Trim();
        if (string.IsNullOrWhiteSpace(key))
            return AuthenticateResult.Fail("API key vazia.");

        var hash = HashKey(key);

        var sensor = await _db.Sensores.FirstOrDefaultAsync(s => s.Ativo && s.ApiKeyHash == hash);
        if (sensor is null)
            return AuthenticateResult.Fail("API key inválida.");

        sensor.UltimoUsoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sensor.Id.ToString()),
            new(ClaimTypes.Name, sensor.Nome),
            new(ClaimTypes.Role, "Sensor"),
        };

       
        claims.Add(new Claim("lugarId", sensor.LugarId.ToString()));


        var identity = new ClaimsIdentity(claims, ApiKeyDefaults.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyDefaults.SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    private static string HashKey(string key)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes); // 64 hex chars
    }
}
