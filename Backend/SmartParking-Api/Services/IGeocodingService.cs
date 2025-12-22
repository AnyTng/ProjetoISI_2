namespace SmartParking_Api.Services;
using SmartParking_Api.Dtos;

public interface IGeocodingService
{
    Task<IReadOnlyList<GeocodeSuggestion>> StructuredAsync(StructuredGeocodeRequest req, CancellationToken ct = default);
}