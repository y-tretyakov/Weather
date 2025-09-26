using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChuhuivWeather.App.Models;
using ChuhuivWeather.App.Services;

namespace ChuhuivWeather.App.ViewModels;

/// <summary>
/// Main view model for the weather application using MVVM pattern
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly OpenMeteoWeatherService _weatherService;
    private readonly CacheService _cacheService;
    private readonly DispatcherTimer _autoRefreshTimer;
    private CancellationTokenSource? _refreshCancellationTokenSource;

    // Auto-refresh interval (1 hour as recommended by Open-Meteo)
    private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromHours(1);

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _location = "Chuhuiv, Kharkiv Oblast";

    [ObservableProperty]
    private WeatherSnapshot? _snapshot;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private DateTimeOffset? _lastUpdated;

    [ObservableProperty]
    private bool _isAutoRefreshEnabled = true;

    /// <summary>
    /// Initializes a new instance of the MainViewModel
    /// </summary>
    public MainViewModel()
    {
        _weatherService = new OpenMeteoWeatherService();
        _cacheService = new CacheService();
        
        // Setup auto-refresh timer
        _autoRefreshTimer = new DispatcherTimer
        {
            Interval = AutoRefreshInterval
        };
        _autoRefreshTimer.Tick += OnAutoRefreshTimer;

        // Initialize with cached data and start refresh
        _ = InitializeAsync();
    }

    /// <summary>
    /// Command to refresh weather data
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        await RefreshWeatherDataAsync();
    }

    /// <summary>
    /// Command to toggle auto-refresh functionality
    /// </summary>
    [RelayCommand]
    private void ToggleAutoRefresh()
    {
        IsAutoRefreshEnabled = !IsAutoRefreshEnabled;
        
        if (IsAutoRefreshEnabled)
        {
            _autoRefreshTimer.Start();
        }
        else
        {
            _autoRefreshTimer.Stop();
        }
    }

    /// <summary>
    /// Determines if refresh command can be executed
    /// </summary>
    private bool CanRefresh() => !IsBusy;

    /// <summary>
    /// Initializes the view model with cached data and starts first refresh
    /// </summary>
    private async Task InitializeAsync()
    {
        // Load cached data first for immediate UI feedback
        await LoadCachedDataAsync();
        
        // Then refresh with latest data from API
        await RefreshWeatherDataAsync();
        
        // Start auto-refresh timer if enabled
        if (IsAutoRefreshEnabled)
        {
            _autoRefreshTimer.Start();
        }
    }

    /// <summary>
    /// Loads cached weather data if available
    /// </summary>
    private async Task LoadCachedDataAsync()
    {
        try
        {
            var cachedData = await _cacheService.LoadFromCacheAsync();
            if (cachedData != null)
            {
                Snapshot = cachedData;
                
                // Get cache info for last updated time
                var cacheInfo = await _cacheService.GetCacheInfoAsync();
                if (cacheInfo != null)
                {
                    LastUpdated = cacheInfo.Timestamp;
                }
            }
        }
        catch (Exception)
        {
            // Ignore cache loading errors
        }
    }

    /// <summary>
    /// Refreshes weather data from the API
    /// </summary>
    private async Task RefreshWeatherDataAsync()
    {
        // Cancel any ongoing refresh operation
        _refreshCancellationTokenSource?.Cancel();
        _refreshCancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsBusy = true;
            Error = null;

            // Update the RefreshCommand can execute state
            RefreshCommand.NotifyCanExecuteChanged();

            var weatherData = await _weatherService.GetWeatherAsync(_refreshCancellationTokenSource.Token);
            
            // Update UI with new data
            Snapshot = weatherData;
            LastUpdated = DateTimeOffset.Now;
            
            // Cache the new data
            await _cacheService.SaveToCacheAsync(weatherData);
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, don't show error
        }
        catch (Exception ex)
        {
            Error = GetUserFriendlyErrorMessage(ex);
            
            // If we have no data and failed to load, try loading from cache
            if (Snapshot == null)
            {
                await LoadCachedDataAsync();
            }
        }
        finally
        {
            IsBusy = false;
            RefreshCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Converts technical exceptions to user-friendly error messages
    /// </summary>
    private static string GetUserFriendlyErrorMessage(Exception exception)
    {
        return exception switch
        {
            TaskCanceledException => "Соединение прервано по таймауту. Проверьте подключение к интернету.",
            System.Net.Http.HttpRequestException => "Ошибка сети. Проверьте подключение к интернету и повторите попытку.",
            _ => "Произошла ошибка при загрузке данных о погоде. Повторите попытку."
        };
    }

    /// <summary>
    /// Handles auto-refresh timer tick
    /// </summary>
    private async void OnAutoRefreshTimer(object? sender, EventArgs e)
    {
        if (!IsBusy && IsAutoRefreshEnabled)
        {
            await RefreshWeatherDataAsync();
        }
    }

    /// <summary>
    /// Disposes the view model resources
    /// </summary>
    public void Dispose()
    {
        _autoRefreshTimer?.Stop();
        
        _refreshCancellationTokenSource?.Cancel();
        _refreshCancellationTokenSource?.Dispose();
        
        _weatherService?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}