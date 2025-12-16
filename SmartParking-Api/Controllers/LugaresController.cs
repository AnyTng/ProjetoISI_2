using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Data;
using SmartParking_Api.Models;

namespace SmartParking_Api.Controllers;

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
            Codigo = dto.Codigo,
            Estado = "Livre" // default
        };

        _db.Lugares.Add(lugar);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLugar), new { id = lugar.Id }, lugar);
    }

    // PUT: api/lugares/5/estado
    // body: { "estado": "Ocupado" }
    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> AtualizarEstado(int id, [FromBody] AtualizarEstadoLugarDto dto)
    {
        var lugar = await _db.Lugares.FirstOrDefaultAsync(l => l.Id == id);
        if (lugar == null)
            return NotFound();

        // Validação simples
        var estado = dto.Estado?.Trim();
        var estadosValidos = new[] { "Livre", "Ocupado", "Indisponivel" };
        if (estado == null || !estadosValidos.Contains(estado))
            return BadRequest($"Estado inválido. Use: {string.Join(", ", estadosValidos)}");

        lugar.Estado = estado;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs usados neste controller
public class CreateLugarDto
{
    public int ParqueId { get; set; }
    public string Codigo { get; set; } = null!;
}

public class AtualizarEstadoLugarDto
{
    public string Estado { get; set; } = null!;
}
