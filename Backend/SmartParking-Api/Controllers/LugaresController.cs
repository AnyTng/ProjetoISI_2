using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Data;
using SmartParking_Api.Models;

namespace SmartParking_Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using SmartParking_Api.Auth;


[ApiController]
[Route("api/[controller]")]
public class LugaresController : ControllerBase
{
    private readonly SmartParkingDbContext _db;

    public LugaresController(SmartParkingDbContext db)
    {
        _db = db;
    }

    // GET: api/lugares/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Lugar>> GetLugar(int id)
    {
        var lugar = await _db.Lugares
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lugar == null)
            return NotFound();

        return Ok(lugar);
    }

    // GET: api/lugares/por-parque/3
    [HttpGet("por-parque/{parqueId:int}")]
    public async Task<ActionResult<IEnumerable<Lugar>>> GetLugaresPorParque(int parqueId)
    {
        var lugares = await _db.Lugares
            .Where(l => l.ParqueId == parqueId)
            .AsNoTracking()
            .ToListAsync();
    
        return Ok(lugares);
    }

    // POST: api/lugares
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Lugar>> CreateLugar([FromBody] CreateLugarDto dto)
    {
        // Verificar se o parque existe
        var parqueExiste = await _db.Parques.AnyAsync(p => p.Id == dto.ParqueId);
        if (!parqueExiste)
            return BadRequest($"Parque {dto.ParqueId} não existe.");

        var lugar = new Lugar
        {
            ParqueId = dto.ParqueId,
            Estado = "Livre" // default
        };

        _db.Lugares.Add(lugar);
        await _db.SaveChangesAsync();
        

        return CreatedAtAction(nameof(GetLugar), new { id = lugar.Id }, lugar);
    }

    // DELETE: api/lugares/5
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLugar(int id)
    {
        var lugar = await _db.Lugares.FirstOrDefaultAsync(l => l.Id == id);
        if (lugar == null)
            return BadRequest($"Lugar {id} não existe.");

        var sensores = await _db.Sensores
            .Where(s => s.LugarId == id)
            .ToListAsync();
        _db.Sensores.RemoveRange(sensores);

        _db.Lugares.Remove(lugar);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/lugares/5/estado
    // body: "Ocupado"
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> AtualizarEstadoAdmin(int id, [FromBody] string estado)
    {
        var lugar = await _db.Lugares.FirstOrDefaultAsync(l => l.Id == id);
        if (lugar == null)
            return NotFound();

        var estadoNormalizado = ValidarEstado(estado);
        if (estadoNormalizado == null)
            return BadRequest($"Estado inválido. Use: {string.Join(", ", EstadosValidos)}");

        lugar.Estado = estadoNormalizado;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/lugares/lugar/estado
    // body: "Ocupado"
    [Authorize(AuthenticationSchemes = ApiKeyDefaults.SchemeName, Roles = "Sensor")]
    [HttpPut("lugar/estado")]
    public async Task<IActionResult> AtualizarEstadoSensor([FromBody] string estado)
    {
        var lugarIdClaim = User.FindFirstValue("lugarId");
        if (!int.TryParse(lugarIdClaim, out var sensorLugarId))
            return Forbid();

        var lugar = await _db.Lugares.FirstOrDefaultAsync(l => l.Id == sensorLugarId);
        if (lugar == null)
            return NotFound();

        var estadoNormalizado = ValidarEstado(estado);
        if (estadoNormalizado == null)
            return BadRequest($"Estado inválido. Use: {string.Join(", ", EstadosValidos)}");

        lugar.Estado = estadoNormalizado;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static readonly string[] EstadosValidos = { "Livre", "Ocupado", "Indisponivel" };

    private static string? ValidarEstado(string? estado)
    {
        var estadoNormalizado = estado?.Trim();
        return estadoNormalizado != null && EstadosValidos.Contains(estadoNormalizado)
            ? estadoNormalizado
            : null;
    }
}

// DTOs usados neste controller
public class CreateLugarDto
{
    public int ParqueId { get; set; }
}
