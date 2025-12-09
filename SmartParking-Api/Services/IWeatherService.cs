using SmartParking_Api.Dtos;

namespace SmartParking_Api.Services;

public interface IWeatherService
{
    Task<WeatherInfoDto?> GetCurrentWeatherAsync(double latitude, double longitude);
}