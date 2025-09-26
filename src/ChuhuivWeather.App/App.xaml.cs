using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ChuhuivWeather.App.Infrastructure.Http;

namespace ChuhuivWeather.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Application startup handler
    /// </summary>
    /// <param name="e">Startup event args</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        // Configure global exception handling
        this.DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        base.OnStartup(e);
    }
    
    /// <summary>
    /// Application exit handler
    /// </summary>
    /// <param name="e">Exit event args</param>
    protected override void OnExit(ExitEventArgs e)
    {
        // Dispose HTTP client resources
        HttpClientFactory.Dispose();
        
        base.OnExit(e);
    }
    
    /// <summary>
    /// Handles unhandled exceptions from the UI thread
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException("UI Thread Exception", e.Exception);
        
        // For now, just mark as handled to prevent app crash
        // In production, you might want to show an error dialog
        e.Handled = true;
    }
    
    /// <summary>
    /// Handles unobserved task exceptions
    /// </summary>
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException("Unobserved Task Exception", e.Exception);
        e.SetObserved();
    }
    
    /// <summary>
    /// Handles unhandled exceptions from other threads
    /// </summary>
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            LogException("Unhandled Exception", exception);
        }
    }
    
    /// <summary>
    /// Logs exceptions for debugging purposes
    /// </summary>
    /// <param name="source">Exception source description</param>
    /// <param name="exception">The exception to log</param>
    private static void LogException(string source, Exception exception)
    {
        // For now, output to debug console
        // In production, you would use a proper logging framework like Serilog
        System.Diagnostics.Debug.WriteLine($"[{source}] {exception}");
        
        // Could also write to a log file here
        try
        {
            var logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ChuhuivWeather",
                "logs");
                
            if (!System.IO.Directory.Exists(logPath))
                System.IO.Directory.CreateDirectory(logPath);
                
            var logFile = System.IO.Path.Combine(logPath, $"errors-{DateTime.Now:yyyy-MM-dd}.log");
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] {exception}\n";
            
            System.IO.File.AppendAllText(logFile, logEntry);
        }
        catch
        {
            // Ignore logging errors to prevent recursive exceptions
        }
    }
}

