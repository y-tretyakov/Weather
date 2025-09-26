using System;
using System.Globalization;
using System.Windows.Data;

namespace ChuhuivWeather.App.Converters;

/// <summary>
/// Converter for formatting temperature values with appropriate units and precision
/// </summary>
public class TemperatureFormatter : IValueConverter
{
    /// <summary>
    /// Converts temperature value to formatted string
    /// </summary>
    /// <param name="value">Temperature value (double)</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Format parameter: "short" for "15°", "long" for "15°C", "large" for big display</param>
    /// <param name="culture">Culture information</param>
    /// <returns>Formatted temperature string</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double temperature)
            return "--°";

        var format = parameter?.ToString()?.ToLower() ?? "short";
        var roundedTemp = Math.Round(temperature);

        return format switch
        {
            "short" => $"{roundedTemp:F0}°",
            "long" => $"{roundedTemp:F0}°C",
            "large" => $"{roundedTemp:F0}°",
            "range" => $"{roundedTemp:F0}°", // For min/max ranges
            _ => $"{roundedTemp:F0}°C"
        };
    }

    /// <summary>
    /// Converts back from formatted string to temperature (not implemented)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Converting back from formatted temperature is not supported.");
    }
}

/// <summary>
/// Converter for wind speed formatting
/// </summary>
public class WindSpeedFormatter : IValueConverter
{
    /// <summary>
    /// Converts wind speed to formatted string
    /// </summary>
    /// <param name="value">Wind speed in km/h (double)</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Format parameter (not used)</param>
    /// <param name="culture">Culture information</param>
    /// <returns>Formatted wind speed string</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double windSpeed)
            return "0 км/ч";

        return $"{Math.Round(windSpeed):F0} км/ч";
    }

    /// <summary>
    /// Converts back from formatted string to wind speed (not implemented)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Converting back from formatted wind speed is not supported.");
    }
}

/// <summary>
/// Converter for pressure formatting
/// </summary>
public class PressureFormatter : IValueConverter
{
    /// <summary>
    /// Converts pressure to formatted string
    /// </summary>
    /// <param name="value">Pressure in hPa (double)</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Format parameter (not used)</param>
    /// <param name="culture">Culture information</param>
    /// <returns>Formatted pressure string</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double pressure)
            return "0 гПа";

        return $"{Math.Round(pressure):F0} гПа";
    }

    /// <summary>
    /// Converts back from formatted string to pressure (not implemented)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Converting back from formatted pressure is not supported.");
    }
}

/// <summary>
/// Converter for humidity percentage formatting
/// </summary>
public class HumidityFormatter : IValueConverter
{
    /// <summary>
    /// Converts humidity to formatted percentage string
    /// </summary>
    /// <param name="value">Humidity percentage (int)</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Format parameter (not used)</param>
    /// <param name="culture">Culture information</param>
    /// <returns>Formatted humidity string</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int humidity)
            return "0%";

        return $"{humidity}%";
    }

    /// <summary>
    /// Converts back from formatted string to humidity (not implemented)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Converting back from formatted humidity is not supported.");
    }
}

/// <summary>
/// Converter for precipitation formatting
/// </summary>
public class PrecipitationFormatter : IValueConverter
{
    /// <summary>
    /// Converts precipitation amount to formatted string
    /// </summary>
    /// <param name="value">Precipitation in mm (double)</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Format parameter (not used)</param>
    /// <param name="culture">Culture information</param>
    /// <returns>Formatted precipitation string</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double precipitation)
            return "0 мм";

        if (precipitation < 0.1)
            return "0 мм";

        return $"{precipitation:F1} мм";
    }

    /// <summary>
    /// Converts back from formatted string to precipitation (not implemented)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Converting back from formatted precipitation is not supported.");
    }
}