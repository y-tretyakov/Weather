using System.Windows;
using ChuhuivWeather.App.ViewModels;

namespace ChuhuivWeather.App.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the MainWindow
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles window closing to properly dispose resources
    /// </summary>
    /// <param name="e">Cancel event args</param>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Dispose the view model if it implements IDisposable
        if (DataContext is IDisposable disposableDataContext)
        {
            disposableDataContext.Dispose();
        }
        
        base.OnClosing(e);
    }
}