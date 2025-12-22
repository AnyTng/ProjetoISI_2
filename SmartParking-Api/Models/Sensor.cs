using System;
using SmartParking_Api.Models;

public class Sensor
{
    public int Id { get; set; }
    public string ApiKeyHash { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoUsoEm { get; set; }
}