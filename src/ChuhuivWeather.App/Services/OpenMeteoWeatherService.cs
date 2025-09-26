using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChuhuivWeather.App.Infrastructure.Http;
using ChuhuivWeather.App.Models;

namespace ChuhuivWeather.App.Services;

/// <summary>
/// Service for retrieving weather data from Open-Meteo API using JSON format
/// </summary>
public class OpenMeteoWeatherService : IDisposable
{
    // Chuhuiv coordinates as specified in the design document
    private const double ChuguyivLatitude = 49.836626;
    private const double ChuguyivLongitude = 36.689939;
    private const string ChuguyivTimezone = "Europe/Kyiv";
    private const string ChuguyivLocationName = "Chuhuiv, Kharkiv Oblast";

    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _requestSemaphore;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the OpenMeteoWeatherService
    /// </summary>
    public OpenMeteoWeatherService()
    {
        _httpClient = HttpClientFactory.Instance;
        _requestSemaphore = new SemaphoreSlim(1, 1);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    /// <summary>
    /// Gets current weather conditions and 3-day forecast for Chuhuiv
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Weather snapshot with current conditions and forecast</returns>
    public async Task<WeatherSnapshot> GetWeatherAsync(CancellationToken cancellationToken = default)
    {
        await _requestSemaphore.WaitAsync(cancellationToken);
        
        try
        {
            var url = BuildApiUrl();
            var responseJson = await GetWeatherDataWithRetryAsync(url, cancellationToken);
            var weatherData = ParseJsonResponse(responseJson);
            
            return weatherData;
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    /// <summary>
    /// Builds the API URL with required parameters for Chuhuiv weather data
    /// </summary>
    private static string BuildApiUrl()
    {
        var currentParams = new[]
        {
            "temperature_2m",
            "apparent_temperature", 
            "relative_humidity_2m",
            "weather_code",
            "cloud_cover",
            "pressure_msl",
            "wind_speed_10m",
            "wind_direction_10m",
            "wind_gusts_10m"
        };

        var dailyParams = new[]
        {
            "weather_code",
            "temperature_2m_max",
            "temperature_2m_min", 
            "precipitation_sum",
            "wind_gusts_10m_max"
        };

        var url = new StringBuilder("v1/forecast?");
        url.Append($"latitude={ChuguyivLatitude.ToString(CultureInfo.InvariantCulture)}&");
        url.Append($"longitude={ChuguyivLongitude.ToString(CultureInfo.InvariantCulture)}&");
        url.Append($"timezone={ChuguyivTimezone}&");
        url.Append("forecast_days=3&");
        url.Append($"current={string.Join(",", currentParams)}&");
        url.Append($"daily={string.Join(",", dailyParams)}");

        return url.ToString();
    }

    /// <summary>
    /// Performs HTTP request with exponential backoff retry strategy
    /// </summary>
    private async Task<string> GetWeatherDataWithRetryAsync(string url, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        var baseDelay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception ex) when (attempt < maxRetries && ShouldRetry(ex))
            {
                var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, cancellationToken);
            }
        }

        // Final attempt without catching exceptions
        using var finalResponse = await _httpClient.GetAsync(url, cancellationToken);
        finalResponse.EnsureSuccessStatusCode();
        return await finalResponse.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Determines if an exception should trigger a retry
    /// </summary>
    private static bool ShouldRetry(Exception exception)
    {
        return exception is HttpRequestException or TaskCanceledException;
    }

    /// <summary>
    /// Parses JSON response data into domain models
    /// </summary>
    private static WeatherSnapshot ParseJsonResponse(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        var current = ParseCurrentConditions(root);
        var forecast = ParseDailyForecast(root);

        return new WeatherSnapshot
        {
            LocationName = ChuguyivLocationName,
            Current = current,
            Next3Days = forecast
        };
    }

    /// <summary>
    /// Parses current weather conditions from JSON data
    /// </summary>
    private static CurrentConditions? ParseCurrentConditions(JsonElement root)
    {
        if (!root.TryGetProperty("current", out var current))
            return null;

        var timeString = current.GetProperty("time").GetString();
        var timeLocal = DateTimeOffset.Parse(timeString!);

        return new CurrentConditions
        {
            TimeLocal = timeLocal,
            TemperatureC = current.GetProperty("temperature_2m").GetDouble(),
            ApparentTemperatureC = current.GetProperty("apparent_temperature").GetDouble(),
            RelativeHumidityPct = current.GetProperty("relative_humidity_2m").GetInt32(),
            WeatherCode = current.GetProperty("weather_code").GetInt32(),
            CloudCoverPct = current.GetProperty("cloud_cover").GetInt32(),
            PressureMslHpa = current.GetProperty("pressure_msl").GetDouble(),
            WindSpeedKmh = current.GetProperty("wind_speed_10m").GetDouble(),
            WindDirectionDeg = current.GetProperty("wind_direction_10m").GetInt32(),
            WindGustKmh = current.GetProperty("wind_gusts_10m").GetDouble()
        };
    }

    /// <summary>
    /// Parses daily forecast from JSON data
    /// </summary>
    private static IReadOnlyList<DailyForecast> ParseDailyForecast(JsonElement root)
    {
        if (!root.TryGetProperty("daily", out var daily))
            return Array.Empty<DailyForecast>();

        var times = daily.GetProperty("time").EnumerateArray().ToArray();
        var weatherCodes = daily.GetProperty("weather_code").EnumerateArray().ToArray();
        var tMin = daily.GetProperty("temperature_2m_min").EnumerateArray().ToArray();
        var tMax = daily.GetProperty("temperature_2m_max").EnumerateArray().ToArray();
        var precipitation = daily.GetProperty("precipitation_sum").EnumerateArray().ToArray();
        var windGusts = daily.GetProperty("wind_gusts_10m_max").EnumerateArray().ToArray();

        var forecasts = new List<DailyForecast>();
        var count = Math.Min(times.Length, 3);

        for (int i = 0; i < count; i++)
        {
            var dateString = times[i].GetString()!;
            var dateLocal = DateOnly.Parse(dateString);

            var forecast = new DailyForecast
            {
                DateLocal = dateLocal,
                WeatherCode = weatherCodes[i].GetInt32(),
                TminC = tMin[i].GetDouble(),
                TmaxC = tMax[i].GetDouble(),
                PrecipitationSumMm = precipitation[i].GetDouble(),
                WindGustMaxKmh = windGusts[i].GetDouble()
            };

            forecasts.Add(forecast);
        }

        return forecasts.AsReadOnly();
    }

    /// <summary>
    /// Disposes the service resources
    /// </summary>
    public void Dispose()
    {
        _requestSemaphore?.Dispose();
        GC.SuppressFinalize(this);
    }
}