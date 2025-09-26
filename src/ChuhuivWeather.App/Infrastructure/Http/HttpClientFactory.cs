using System;
using System.Net.Http;

namespace ChuhuivWeather.App.Infrastructure.Http;

/// <summary>
/// Factory for creating and configuring HTTP clients for Open-Meteo API communication
/// </summary>
public static class HttpClientFactory
{
    private static readonly Lazy<HttpClient> _lazyHttpClient = new(() => CreateConfiguredHttpClient());

    /// <summary>
    /// Gets a configured HTTP client instance optimized for Open-Meteo API requests
    /// </summary>
    public static HttpClient Instance => _lazyHttpClient.Value;

    /// <summary>
    /// Creates and configures a new HttpClient instance for Open-Meteo API
    /// </summary>
    /// <returns>Configured HttpClient instance</returns>
    private static HttpClient CreateConfiguredHttpClient()
    {
        var handler = new HttpClientHandler()
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };
        var httpClient = new HttpClient(handler);

        // Configure timeouts to prevent hanging requests
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        // Set User-Agent header for API identification as recommended by Open-Meteo
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ChuhuivWeather/1.0");

        // Set base address for Open-Meteo API
        httpClient.BaseAddress = new Uri("https://api.open-meteo.com/");

        // Accept gzip compression for better performance
        httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
        httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");

        return httpClient;
    }

    /// <summary>
    /// Disposes the HTTP client resources
    /// </summary>
    public static void Dispose()
    {
        if (_lazyHttpClient.IsValueCreated)
        {
            _lazyHttpClient.Value.Dispose();
        }
    }
}