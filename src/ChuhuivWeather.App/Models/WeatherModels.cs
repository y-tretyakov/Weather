using System;
using System.Collections.Generic;

namespace ChuhuivWeather.App.Models;

/// <summary>
/// Represents current weather conditions for a specific location
/// </summary>
public record CurrentConditions
{
    /// <summary>
    /// Local time of measurement
    /// </summary>
    public DateTimeOffset TimeLocal { get; init; }

    /// <summary>
    /// Temperature in degrees Celsius
    /// </summary>
    public double TemperatureC { get; init; }

    /// <summary>
    /// Apparent temperature in degrees Celsius
    /// </summary>
    public double ApparentTemperatureC { get; init; }

    /// <summary>
    /// Relative humidity as percentage
    /// </summary>
    public int RelativeHumidityPct { get; init; }

    /// <summary>
    /// WMO weather code
    /// </summary>
    public int WeatherCode { get; init; }

    /// <summary>
    /// Cloud cover as percentage
    /// </summary>
    public int CloudCoverPct { get; init; }

    /// <summary>
    /// Pressure at mean sea level in hectopascals
    /// </summary>
    public double PressureMslHpa { get; init; }

    /// <summary>
    /// Wind speed in kilometers per hour
    /// </summary>
    public double WindSpeedKmh { get; init; }

    /// <summary>
    /// Wind direction in degrees
    /// </summary>
    public int WindDirectionDeg { get; init; }

    /// <summary>
    /// Wind gusts in kilometers per hour
    /// </summary>
    public double WindGustKmh { get; init; }
}

/// <summary>
/// Represents weather forecast for a single day
/// </summary>
public record DailyForecast
{
    /// <summary>
    /// Date of the forecast
    /// </summary>
    public DateOnly DateLocal { get; init; }

    /// <summary>
    /// WMO weather code for the day
    /// </summary>
    public int WeatherCode { get; init; }

    /// <summary>
    /// Minimum temperature in degrees Celsius
    /// </summary>
    public double TminC { get; init; }

    /// <summary>
    /// Maximum temperature in degrees Celsius
    /// </summary>
    public double TmaxC { get; init; }

    /// <summary>
    /// Total precipitation sum in millimeters
    /// </summary>
    public double PrecipitationSumMm { get; init; }

    /// <summary>
    /// Maximum wind gusts in kilometers per hour
    /// </summary>
    public double WindGustMaxKmh { get; init; }
}

/// <summary>
/// Aggregates complete weather information for a location
/// </summary>
public record WeatherSnapshot
{
    /// <summary>
    /// Name of the location
    /// </summary>
    public string LocationName { get; init; } = string.Empty;

    /// <summary>
    /// Current weather conditions
    /// </summary>
    public CurrentConditions? Current { get; init; }

    /// <summary>
    /// Weather forecast for the next 3 days
    /// </summary>
    public IReadOnlyList<DailyForecast> Next3Days { get; init; } = Array.Empty<DailyForecast>();
}

/// <summary>
/// Cached weather data with expiration information
/// </summary>
public record CachedWeatherData
{
    /// <summary>
    /// The cached weather snapshot
    /// </summary>
    public WeatherSnapshot? Data { get; init; }

    /// <summary>
    /// Timestamp when the data was cached
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Timestamp when the cache expires
    /// </summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// Indicates whether the cached data is still valid
    /// </summary>
    public bool IsValid => DateTimeOffset.UtcNow < ExpiresAt;
}