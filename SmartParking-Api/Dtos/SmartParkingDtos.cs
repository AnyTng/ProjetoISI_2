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


