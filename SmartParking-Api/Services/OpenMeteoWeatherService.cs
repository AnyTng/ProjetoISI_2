using System.Globalization;
using System.Text.Json;
using SmartParking_Api.Dtos;

namespace SmartParking_Api.Services;

public class OpenMeteoWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;

    public OpenMeteoWeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherInfoDto?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);

        var url =
            $"https://api.open-meteo.com/v1/forecast" +
            $"?latitude={lat}&longitude={lon}" +
            $"&current=temperature_2m,relative_humidity_2m,precipitation" +
            $"&timezone=auto";


        using var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        if (!doc.RootElement.TryGetProperty("current", out var current))
            return null;

        var temperatura = current.GetProperty("temperature_2m").GetDouble();
        var humidade = current.GetProperty("relative_humidity_2m").GetDouble();
        var precipitacao = current.GetProperty("precipitation").GetDouble();
        
        var estaAChover = precipitacao > 0.1;
        
        return new WeatherInfoDto
        {
            Descricao = estaAChover ? "A chover" : "Sem chuva",
            Temperatura = temperatura,
            Humidade = humidade
        };
    }
}