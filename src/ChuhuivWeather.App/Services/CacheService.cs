using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ChuhuivWeather.App.Models;

namespace ChuhuivWeather.App.Services;

/// <summary>
/// Service for caching weather data to local storage for offline functionality
/// </summary>
public class CacheService
{
    private readonly string _cacheDirectory;
    private readonly string _cacheFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    // Cache lifetime constants as specified in design document
    private static readonly TimeSpan CurrentConditionsCacheLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ForecastCacheLifetime = TimeSpan.FromHours(24);

    /// <summary>
    /// Initializes a new instance of the CacheService
    /// </summary>
    public CacheService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _cacheDirectory = Path.Combine(localAppData, "ChuhuivWeather");
        _cacheFilePath = Path.Combine(_cacheDirectory, "cache.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        EnsureCacheDirectoryExists();
    }

    /// <summary>
    /// Saves weather data to cache with appropriate expiration time
    /// </summary>
    /// <param name="weatherData">Weather data to cache</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SaveToCacheAsync(WeatherSnapshot weatherData)
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            
            // Use shorter cache lifetime for current conditions, longer for forecast
            var expirationTime = now.Add(CurrentConditionsCacheLifetime);
            
            var cachedData = new CachedWeatherData
            {
                Data = weatherData,
                Timestamp = now,
                ExpiresAt = expirationTime
            };

            var jsonData = JsonSerializer.Serialize(cachedData, _jsonOptions);
            await File.WriteAllTextAsync(_cacheFilePath, jsonData);
        }
        catch (Exception)
        {
            // Silently ignore cache write errors to not interrupt the main flow
        }
    }

    /// <summary>
    /// Loads cached weather data if available and still valid
    /// </summary>
    /// <returns>Cached weather data or null if no valid cache exists</returns>
    public async Task<WeatherSnapshot?> LoadFromCacheAsync()
    {
        try
        {
            if (!File.Exists(_cacheFilePath))
                return null;

            var jsonData = await File.ReadAllTextAsync(_cacheFilePath);
            var cachedData = JsonSerializer.Deserialize<CachedWeatherData>(jsonData, _jsonOptions);

            if (cachedData?.IsValid == true)
                return cachedData.Data;

            // Cache is expired, delete the file
            await DeleteCacheAsync();
            return null;
        }
        catch (Exception)
        {
            // If there's any error reading cache, treat as no cache available
            return null;
        }
    }

    /// <summary>
    /// Checks if valid cached data exists
    /// </summary>
    /// <returns>True if valid cache exists, false otherwise</returns>
    public async Task<bool> HasValidCacheAsync()
    {
        var cachedData = await LoadFromCacheAsync();
        return cachedData != null;
    }

    /// <summary>
    /// Gets information about the cached data
    /// </summary>
    /// <returns>Cache information or null if no cache exists</returns>
    public async Task<CachedWeatherData?> GetCacheInfoAsync()
    {
        try
        {
            if (!File.Exists(_cacheFilePath))
                return null;

            var jsonData = await File.ReadAllTextAsync(_cacheFilePath);
            return JsonSerializer.Deserialize<CachedWeatherData>(jsonData, _jsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Deletes the cache file
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task DeleteCacheAsync()
    {
        try
        {
            if (File.Exists(_cacheFilePath))
            {
                await Task.Run(() => File.Delete(_cacheFilePath));
            }
        }
        catch (Exception)
        {
            // Silently ignore cache deletion errors
        }
    }

    /// <summary>
    /// Gets the size of the cache file in bytes
    /// </summary>
    /// <returns>Cache file size in bytes, or 0 if file doesn't exist</returns>
    public long GetCacheSizeBytes()
    {
        try
        {
            if (File.Exists(_cacheFilePath))
            {
                var fileInfo = new FileInfo(_cacheFilePath);
                return fileInfo.Length;
            }
            return 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// Ensures the cache directory exists
    /// </summary>
    private void EnsureCacheDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }
        catch (Exception)
        {
            // If we can't create the directory, caching will be disabled
        }
    }
}