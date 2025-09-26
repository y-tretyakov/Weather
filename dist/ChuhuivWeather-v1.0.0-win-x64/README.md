# Chuhuiv Weather - WPF Application

A desktop WPF application for displaying current weather conditions and 3-day forecast for Chuhuiv, Kharkiv Oblast, Ukraine, using the Open-Meteo API.

## Features

- **Current Weather Display**: Shows temperature, feels-like temperature, humidity, wind speed, pressure, and cloud cover
- **3-Day Forecast**: Daily weather predictions with min/max temperatures, precipitation, and wind gusts
- **Weather Icons**: Emoji-based weather condition indicators using WMO weather codes
- **Auto-Refresh**: Automatic data updates every hour (configurable)
- **Offline Support**: Caches last weather data for offline viewing
- **Russian Localization**: User interface in Russian language
- **Error Handling**: Comprehensive error handling with retry logic
- **Modern UI**: Clean WPF interface with shadow effects and modern styling

## Technology Stack

- **.NET 9** - Target framework
- **WPF** - User interface framework
- **MVVM Pattern** - Using CommunityToolkit.Mvvm
- **Open-Meteo API** - Weather data source
- **JSON API** - Data format (FlatBuffers ready for future optimization)

## Architecture

### Project Structure
```
src/ChuhuivWeather.App/
├── Views/                          # WPF Views
│   ├── MainWindow.xaml            # Main application window
│   └── MainWindow.xaml.cs         # Code-behind
├── ViewModels/                     # MVVM ViewModels
│   └── MainViewModel.cs           # Main view model with commands and properties
├── Services/                       # Business logic services
│   ├── OpenMeteoWeatherService.cs # Weather API integration
│   └── CacheService.cs            # Local data caching
├── Models/                         # Domain models
│   └── WeatherModels.cs           # Weather data structures
├── Converters/                     # Value converters for UI
│   ├── WeatherCodeToIconConverter.cs # Weather code to emoji mapping
│   └── ValueFormatters.cs         # Temperature, wind, pressure formatters
├── Infrastructure/                 # Infrastructure components
│   └── Http/
│       └── HttpClientFactory.cs   # HTTP client configuration
├── Assets/                         # Application assets
└── App.xaml / App.xaml.cs         # Application entry point
```

### Key Components

#### Data Models
- **`CurrentConditions`** - Current weather data
- **`DailyForecast`** - Single day forecast data  
- **`WeatherSnapshot`** - Complete weather information
- **`CachedWeatherData`** - Cached data with expiration

#### Services
- **`OpenMeteoWeatherService`** - Handles API communication with retry logic
- **`CacheService`** - Manages local data storage and retrieval

#### UI Features
- **Left Panel**: Current conditions with large temperature display
- **Right Panel**: 3-day forecast cards
- **Refresh Button**: Manual data update
- **Loading Indicators**: Visual feedback during data fetching
- **Error Display**: User-friendly error messages

## API Integration

### Open-Meteo API Configuration
- **Location**: Chuhuiv (49.836626°N, 36.689939°E)
- **Timezone**: Europe/Kyiv
- **Forecast Days**: 3
- **Update Frequency**: 1 hour (respecting API guidelines)

### Weather Parameters
**Current Conditions:**
- Temperature at 2m height
- Apparent temperature
- Relative humidity
- Weather code (WMO standard)
- Cloud cover percentage
- Mean sea level pressure
- Wind speed and direction at 10m
- Wind gusts

**Daily Forecast:**
- Weather code
- Min/max temperatures
- Precipitation sum
- Maximum wind gusts

## Caching Strategy

- **Cache Location**: `%LOCALAPPDATA%\ChuhuivWeather\cache.json`
- **Cache Lifetime**: 30 minutes for current conditions
- **Offline Mode**: Shows cached data when network unavailable
- **Automatic Cleanup**: Expired cache files are automatically removed

## Error Handling

- **Retry Logic**: Exponential backoff with 3 retry attempts
- **Network Timeouts**: 10-second timeout for API requests
- **Graceful Degradation**: Falls back to cached data on errors
- **User-Friendly Messages**: Translates technical errors to readable Russian text

## Building and Running

### Prerequisites
- .NET 9.0 SDK
- Windows 10 version 1809 or newer

### Build Commands
```bash
# Navigate to project directory
cd src/ChuhuivWeather.App

# Restore packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run
```

### NuGet Packages
- `CommunityToolkit.Mvvm` (8.4.0) - MVVM framework
- `openmeteo_sdk` (1.18.6) - Open-Meteo FlatBuffers schemas

## Performance Optimizations

- **HTTP Connection Reuse**: Persistent HTTP client with keep-alive
- **Efficient JSON Parsing**: Using System.Text.Json for fast parsing
- **Memory Management**: Proper disposal patterns for resources
- **Background Operations**: Non-blocking UI during API calls
- **Smart Caching**: Reduces API calls while maintaining fresh data

## Future Enhancements

- **FlatBuffers Support**: Optimize to use binary format for better performance
- **Multiple Locations**: Support for different cities
- **Extended Forecast**: 7-day or longer forecasts
- **Weather Alerts**: Severe weather notifications
- **Themes**: Dark/light theme support
- **Detailed Charts**: Temperature and precipitation graphs

## License

This project uses the Open-Meteo API under their free non-commercial license terms.

## Author

Created as part of a weather application development task, following modern .NET and WPF best practices.