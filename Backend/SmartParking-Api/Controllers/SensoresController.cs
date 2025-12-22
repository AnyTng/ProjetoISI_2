using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking_Api.Data;
using SmartParking_Api.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SmartParking_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensoresController : ControllerBase
{
    private readonly SmartParkingDbContext _db;

    public SensoresController(SmartParkingDbContext db) => _db = db;
    
    
    public record CreateSensorRequest(int LugarId);


    
    [Authorize (Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sensores = await _db.Sensores
            .AsNoTracking()
            .ToListAsync();
        
        return Ok(sensores);
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateSensorRequest req)
    {
        var lugarExiste = await _db.Lugares.AnyAsync(l => l.Id == req.LugarId);
        if (!lugarExiste)
            return BadRequest($"Lugar {req.LugarId} não existe.");

        var apiKey = GenerateApiKey();
        var hash = HashKey(apiKey);

        var sensor = new Sensor
        {
            LugarId = req.LugarId,
            ApiKeyHash = hash
        };


        _db.Sensores.Add(sensor);
        await _db.SaveChangesAsync();
        
        return Ok(new { sensor.Id, sensor.LugarId, apiKey });

    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var sensor = await _db.Sensores.FindAsync(id);
        if (sensor == null)
            return BadRequest($"Sensor {id} não encontrado");

        _db.Sensores.Remove(sensor);
        await _db.SaveChangesAsync();

        return NoContent();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/regenerate-apikey")]
    public async Task<IActionResult> RegenerateApiKey(int id)
    {
        var sensor = await _db.Sensores.FindAsync(id);
        if (sensor == null) 
            return BadRequest($"Sensor {id} não existe.");
        
        var apiKey = GenerateApiKey();
        sensor.ApiKeyHash = HashKey(apiKey);
        await _db.SaveChangesAsync();
        
        return Ok(new { sensor.Id, sensor.LugarId, apiKey });
    }
    
    private static string GenerateApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string HashKey(string key)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes);
    }
}
