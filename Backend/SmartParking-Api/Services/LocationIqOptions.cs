namespace SmartParking_Api.Services;

public class LocationIqOptions
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://eu1.locationiq.com";
    public string? CountryCodes { get; set; } 
}

