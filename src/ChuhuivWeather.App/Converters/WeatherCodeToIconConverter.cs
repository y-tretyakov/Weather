using System;
using System.Globalization;
using System.Windows.Data;

namespace ChuhuivWeather.App.Converters;

/// <summary>
/// Converter that maps WMO weather codes to weather icons/emojis
/// </summary>
public class WeatherCodeToIconConverter : IValueConverter
{
    /// <summary>
    /// Converts WMO weather code to corresponding weather icon
    /// </summary>
    /// <param name="value">WMO weather code (int)</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Converter parameter (not used)</param>
    /// <param name="culture">Culture information (not used)</param>
    /// <returns>Weather icon string</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int weatherCode)
            return "❓"; // Unknown weather

        return weatherCode switch
        {
            // Clear sky
            0 => "☀️",
            
            // Mainly clear, partly cloudy, and overcast
            1 => "🌤️", // Mainly clear
            2 => "⛅", // Partly cloudy  
            3 => "☁️", // Overcast
            
            // Fog and depositing rime fog
            45 => "🌫️", // Fog
            48 => "🌫️", // Depositing rime fog
            
            // Drizzle: Light, moderate, and dense intensity
            51 => "🌦️", // Light drizzle
            53 => "🌦️", // Moderate drizzle
            55 => "🌦️", // Dense drizzle
            
            // Freezing Drizzle: Light and dense intensity
            56 => "🌧️", // Light freezing drizzle
            57 => "🌧️", // Dense freezing drizzle
            
            // Rain: Slight, moderate and heavy intensity
            61 => "🌧️", // Slight rain
            63 => "🌧️", // Moderate rain
            65 => "🌧️", // Heavy rain
            
            // Freezing Rain: Light and heavy intensity
            66 => "🌧️", // Light freezing rain
            67 => "🌧️", // Heavy freezing rain
            
            // Snow fall: Slight, moderate, and heavy intensity
            71 => "🌨️", // Slight snow fall
            73 => "🌨️", // Moderate snow fall
            75 => "🌨️", // Heavy snow fall
            
            // Snow grains
            77 => "🌨️", // Snow grains
            
            // Rain showers: Slight, moderate, and violent
            80 => "🌦️", // Slight rain showers
            81 => "🌧️", // Moderate rain showers
            82 => "🌧️", // Violent rain showers
            
            // Snow showers slight and heavy
            85 => "🌨️", // Slight snow showers
            86 => "🌨️", // Heavy snow showers
            
            // Thunderstorm: Slight or moderate
            95 => "⛈️", // Thunderstorm
            
            // Thunderstorm with slight and heavy hail
            96 => "⛈️", // Thunderstorm with slight hail
            99 => "⛈️", // Thunderstorm with heavy hail
            
            // Default case for unknown codes
            _ => "❓"
        };
    }

    /// <summary>
    /// Converts back from icon to weather code (not implemented)
    /// </summary>
    /// <param name="value">Icon value</param>
    /// <param name="targetType">Target type</param>
    /// <param name="parameter">Converter parameter</param>
    /// <param name="culture">Culture information</param>
    /// <returns>Not supported</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Converting back from icon to weather code is not supported.");
    }
}

/// <summary>
/// Converter that maps WMO weather codes to descriptive text
/// </summary>
public class WeatherCodeToDescriptionConverter : IValueConverter
{
    /// <summary>
    /// Converts WMO weather code to weather description in Russian
    /// </summary>
    /// <param name="value">WMO weather code (int)</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Converter parameter (not used)</param>
    /// <param name="culture">Culture information (not used)</param>
    /// <returns>Weather description string</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int weatherCode)
            return "Неизвестно";

        return weatherCode switch
        {
            0 => "Ясно",
            1 => "В основном ясно",
            2 => "Переменная облачность",
            3 => "Пасмурно",
            45 => "Туман",
            48 => "Туман с изморозью",
            51 => "Легкая морось",
            53 => "Умеренная морось",
            55 => "Сильная морось",
            56 => "Легкая ледяная морось",
            57 => "Сильная ледяная морось",
            61 => "Небольшой дождь",
            63 => "Умеренный дождь",
            65 => "Сильный дождь",
            66 => "Небольшой ледяной дождь",
            67 => "Сильный ледяной дождь",
            71 => "Небольшой снег",
            73 => "Умеренный снег",
            75 => "Сильный снег",
            77 => "Снежная крупа",
            80 => "Небольшие ливни",
            81 => "Умеренные ливни",
            82 => "Сильные ливни",
            85 => "Небольшой снегопад",
            86 => "Сильный снегопад",
            95 => "Гроза",
            96 => "Гроза с градом",
            99 => "Гроза с крупным градом",
            _ => "Неизвестно"
        };
    }

    /// <summary>
    /// Converts back from description to weather code (not implemented)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Converting back from description to weather code is not supported.");
    }
}