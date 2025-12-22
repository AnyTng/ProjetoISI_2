using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking_Api.Dtos;
using SmartParking_Api.Services;

namespace SmartParking_Api.Controllers;

[ApiController]

[Route("api/[controller]")]
public class GeocodingController : ControllerBase
{
    private readonly IGeocodingService _geo;
    public GeocodingController(IGeocodingService geo) => _geo = geo;
    
    [Authorize (Roles = "Admin")]
    [HttpPost("structured")]
    public async Task<ActionResult<IReadOnlyList<GeocodeSuggestion>>> Structured([FromBody] StructuredGeocodeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Street))
            return BadRequest("a rua é obrigatória.");

        var results = await _geo.StructuredAsync(req, ct);
        return Ok(results);
    }
}