using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Data;
using SmartParking_Api.Dtos;
using SmartParking_Api.Models;
using SmartParking_Api.Services;
using SmartParking_Api.Services.Soap;

namespace SmartParking_Api.Services.Soap;

public class SmartParkingSoapService : ISmartParkingSoapService
{
    private readonly SmartParkingDbContext _db;
    private readonly IWeatherService _weather;

    public SmartParkingSoapService(SmartParkingDbContext db, IWeatherService weather)
    {
        _db = db;
        _weather = weather;
    }

    public async Task<ParqueResumoDto?> GetParqueResumoAsync(int parqueId)
    {
        var parque = await _db.Parques
            .Include(p => p.Lugares)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == parqueId);

        if (parque == null)
            return null;

        var total = parque.Lugares.Count;
        var livres = parque.Lugares.Count(l => l.Estado == "Livre");
        var ocupados = parque.Lugares.Count(l => l.Estado == "Ocupado");
        var indisponiveis = parque.Lugares.Count(l => l.Estado == "Indisponivel");

        var taxaOcupacao = total == 0
            ? 0
            : (double)ocupados / total * 100.0;

        WeatherInfoDto? meteo = null;
        if (parque.Latitude.HasValue && parque.Longitude.HasValue)
        {
            try
            {
                meteo = await _weather.GetCurrentWeatherAsync(
                    parque.Latitude.Value,
                    parque.Longitude.Value);
            }
            catch
            {
                // não rebentar o serviço se a API externa falhar
            }
        }

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

    public async Task<List<Lugar>> GetLugaresPorParqueAsync(int parqueId)
    {
        var lugares = await _db.Lugares
            .Where(l => l.ParqueId == parqueId)
            .AsNoTracking()
            .ToListAsync();

        return lugares;
    }
}
