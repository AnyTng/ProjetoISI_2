using System.Collections.Generic;

namespace SmartParking_Api.Dtos;

public class ParqueResumoDto
{
    public int ParqueId { get; set; }
    public string Nome { get; set; } = null!;
    public int TotalLugares { get; set; }
    public int LugaresLivres { get; set; }
    public int LugaresOcupados { get; set; }
    public int LugaresIndisponiveis { get; set; }
    public double TaxaOcupacao { get; set; }

    public WeatherInfoDto? Meteorologia { get; set; }
}

public class WeatherInfoDto
{
    public string Descricao { get; set; } = "";
    public double Temperatura { get; set; }
    public double Humidade { get; set; }
}

public class ParqueDto {
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Endereco { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<LugarDto> Lugares { get; set; } = [];
}

public record CreateParqueRequest(
    string Nome,
    string Endereco,
    double? Latitude,
    double? Longitude
);

public class LugarDto {
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Estado { get; set; } = "";
    public int ParqueId { get; set; }
}

public record StructuredGeocodeRequest(
    string Street,
    string? PostalCode,
    string? City,
    string? Country
);

public record GeocodeSuggestion(
    string DisplayName,
    double Lat,
    double Lon,
    string? PlaceId
);
