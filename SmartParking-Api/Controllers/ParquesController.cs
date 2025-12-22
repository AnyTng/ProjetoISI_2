using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Data;
using SmartParking_Api.Dtos;
using SmartParking_Api.Models;
using SmartParking_Api.Services;
namespace SmartParking_Api.Controllers;



[ApiController]
[Route("api/[controller]")]
public class ParquesController : ControllerBase
{
    private readonly SmartParkingDbContext _db;
    private readonly IWeatherService _weather;
    
    public ParquesController(SmartParkingDbContext db, IWeatherService weather)
    {
        _db = db;
        _weather = weather;
    }

    // GET: api/parques/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ParqueDto>> GetParque(int id)
    {
        var parque = await _db.Parques
            .Include(p => p.Lugares)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parque == null) return NotFound();

        var dto = new ParqueDto {
            Id = parque.Id,
            Nome = parque.Nome,
            Endereco = parque.Endereco,
            Latitude = parque.Latitude,
            Longitude = parque.Longitude,
            Lugares = parque.Lugares.Select(l => new LugarDto {
                Id = l.Id,
                ParqueId = l.ParqueId,
                Codigo = l.Codigo,
                Estado = l.Estado
            }).ToList()
        };

        return Ok(dto);
    }


    // Delete: api/Parques
    [Authorize (Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteParque(int id)
    {
        var parque = await _db.Parques
            .Include(p => p.Lugares)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parque == null)
            return NotFound();

        _db.Lugares.RemoveRange(parque.Lugares);
        _db.Parques.Remove(parque);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/parques
    [Authorize (Roles = "Admin")]
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
    [HttpGet("resumo")]
    public async Task<ActionResult<IEnumerable<ParqueResumoDto>>> GetResumoParques()
    {
        var parques = await _db.Parques
            .Include(p => p.Lugares)
            .AsNoTracking()
            .ToListAsync();
        

        var lista = new List<ParqueResumoDto>();

        foreach (var parque in parques)
        {
            WeatherInfoDto? meteo = null;
            try
            {
                meteo = await _weather.GetCurrentWeatherAsync(parque.Latitude, parque.Longitude);
            }
            catch
            {
                // se falhar para este parque, segue sem meteo
            }

            lista.Add(CriarResumoParque(parque, meteo));
        }

        return Ok(lista);
    }

    
    //GET api/5/Resumo
    [HttpGet("{id:int}/resumo")]
    public async Task<ActionResult<ParqueResumoDto>> GetResumoParque(int id)
    {
        var parque = await _db.Parques
            .Include(p => p.Lugares)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parque == null)
            return NotFound();

        WeatherInfoDto? meteo = null;
        try
        {
            meteo = await _weather.GetCurrentWeatherAsync(parque.Latitude, parque.Longitude);
        }
        catch { /* log opcional */ }

        var dto = CriarResumoParque(parque, meteo);
        return Ok(dto);
    }
    

    private static ParqueResumoDto CriarResumoParque(Parque parque, WeatherInfoDto? meteo = null)
    {
        var total = parque.Lugares.Count;
        var livres = parque.Lugares.Count(l => l.Estado == "Livre");
        var ocupados = parque.Lugares.Count(l => l.Estado == "Ocupado");
        var indisponiveis = parque.Lugares.Count(l => l.Estado == "Indisponivel");

        var taxaOcupacao = total == 0
            ? 0
            : (double)ocupados / total * 100.0;

        return new ParqueResumoDto
        {
            ParqueId = parque.Id,
            Nome = parque.Nome,
            TotalLugares = total,
            LugaresLivres = livres,
            LugaresOcupados = ocupados,
            LugaresIndisponiveis = indisponiveis,
            TaxaOcupacao = Math.Round(taxaOcupacao, 2),
            Meteorologia = meteo
        };
    }

}
