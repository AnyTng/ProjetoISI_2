using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Data;
using SmartParking_Api.Dtos;
using SmartParking_Api.Models;

namespace SmartParking_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParquesController : ControllerBase
{
    private readonly SmartParkingDbContext _db;

    public ParquesController(SmartParkingDbContext db)
    {
        _db = db;
    }

    // GET: api/parques
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Parque>>> GetParques()
    {
        var parques = await _db.Parques.AsNoTracking().ToListAsync();
        return Ok(parques);
    }

    // GET: api/parques/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Parque>> GetParque(int id)
    {
        var parque = await _db.Parques
            .Include(p => p.Lugares)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parque == null)
            return NotFound();

        return Ok(parque);
    }

    // POST: api/parques
    [HttpPost]
    public async Task<ActionResult<Parque>> CreateParque([FromBody] Parque parque)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _db.Parques.Add(parque);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetParque), new { id = parque.Id }, parque);
    }
    
    // GET: api/parques/5/lugares
    [HttpGet("{id:int}/lugares")]
    public async Task<ActionResult<IEnumerable<Lugar>>> GetLugaresDoParque(int id)
    {
        var existe = await _db.Parques.AnyAsync(p => p.Id == id);
        if (!existe)
            return NotFound();

        var lugares = await _db.Lugares
            .Where(l => l.ParqueId == id)
            .AsNoTracking()
            .ToListAsync();

        return Ok(lugares);
    }

    // GET: api/resumo
    [HttpGet("resumo") ]
    public async Task<ActionResult<IEnumerable<ParqueResumoDto>>> GetResumoParques()
    {
        var resumo = await _db.Parques
            .AsNoTracking()
            .Select(p => new ParqueResumoDto
            {
                ParqueId = p.Id,
                Nome = p.Nome,
                TotalLugares = p.Lugares.Count,
                LugaresLivres = p.Lugares.Count(l => l.Estado == "Livre"),
                LugaresOcupados = p.Lugares.Count(l => l.Estado == "Ocupado"),
                LugaresIndisponiveis = p.Lugares.Count(l => l.Estado == "Indisponivel"),
                TaxaOcupacao = p.Lugares.Count == 0 
                ? 0 
                : (double)p.Lugares.Count(l => l.Estado == "Ocupado") / p.Lugares.Count * 100.0

            })
            .ToListAsync();
            if (resumo.Count == 0) return NotFound();
        
        return Ok(resumo);
    }
    
    //GET api/5/Resumo
    [HttpGet("{id:int}/resumo")]
    public async Task<ActionResult<ParqueResumoDto>> GetResumoParque(int id)
    {
        var resumo = await _db.Parques
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ParqueResumoDto
            {
                ParqueId = p.Id,
                Nome = p.Nome,
                TotalLugares = p.Lugares.Count,
                LugaresLivres = p.Lugares.Count(l => l.Estado == "Livre"),
                LugaresOcupados = p.Lugares.Count(l => l.Estado == "Ocupado"),
                LugaresIndisponiveis = p.Lugares.Count(l => l.Estado == "Indisponivel"),
                TaxaOcupacao = p.Lugares.Count == 0 
                ? 0 
                : (double)p.Lugares.Count(l => l.Estado == "Ocupado") / p.Lugares.Count * 100.0

            })
            .FirstOrDefaultAsync();
        
        if (resumo == null) return NotFound();
            
        return Ok(resumo);
        
    }
    
}
