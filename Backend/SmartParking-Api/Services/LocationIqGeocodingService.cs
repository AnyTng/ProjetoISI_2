using Microsoft.Extensions.Options;
using SmartParking_Api.Dtos;
using System.Globalization;
using System.Text.Json;

namespace SmartParking_Api.Services;

public class LocationIqGeocodingService : IGeocodingService
{
    private readonly HttpClient _http;
    private readonly LocationIqOptions _opt;

    public LocationIqGeocodingService(HttpClient http, IOptions<LocationIqOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public async Task<IReadOnlyList<GeocodeSuggestion>> StructuredAsync(StructuredGeocodeRequest req, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["key"] = _opt.ApiKey,
            ["format"] = "json",
            ["limit"] = 10.ToString(),
            ["street"] = req.Street,
            ["postalcode"] = req.PostalCode,
            ["city"] = req.City,
            ["country"] = req.Country,
            ["countrycodes"] = _opt.CountryCodes 
        };

        var url = "/v1/search/structured" + ToQueryString(query);
        using var resp = await _http.GetAsync(url, ct);

        if (!resp.IsSuccessStatusCode)
            return Array.Empty<GeocodeSuggestion>();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var results = JsonSerializer.Deserialize<List<LocationIqResult>>(json);

        if (results is null || results.Count == 0)
            return Array.Empty<GeocodeSuggestion>();

        var outList = new List<GeocodeSuggestion>();
        foreach (var r in results)
        {
            if (!double.TryParse(r.lat, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat)) continue;
            if (!double.TryParse(r.lon, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon)) continue;

            outList.Add(new GeocodeSuggestion(
                DisplayName: r.display_name ?? "",
                Lat: lat,
                Lon: lon,
                PlaceId: r.place_id
            ));
        }

        return outList;
    }

    private static string ToQueryString(Dictionary<string, string?> dict)
    {
        var parts = dict
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return "?" + string.Join("&", parts);
    }

    private sealed record LocationIqResult(string? place_id, string? lat, string? lon, string? display_name);
}
