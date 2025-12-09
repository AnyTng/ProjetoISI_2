namespace SmartParking_Api.Models;

public class Lugar
{
    public int Id { get; set; }
    public int ParqueId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Estado { get; set; } = "Livre"; // Livre / Ocupado / Indisponivel

    public Parque? Parque { get; set; }
}