namespace SmartParking_Api.Models;

public class Parque
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Endereco { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public ICollection<Lugar> Lugares { get; set; } = new List<Lugar>();
}