using System;
using System.Windows;

namespace TobisChecklist;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var app = new App();

        // Register global exception handlers early
        app.DispatcherUnhandledException += (s, e) =>
        {
            HandleGlobalException(e.Exception);
            e.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception ?? new Exception("Unknown unhandled exception");
            HandleGlobalException(ex);
        };

        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            try
            {
                HandleGlobalException(e.Exception);
                e.SetObserved();
            }
            catch { }
        };

        app.MainWindow = new MainWindow();
        app.MainWindow.Show();
        app.Run();
    }

    private static void HandleGlobalException(Exception ex)
    {
        try
        {
            var msg = ex?.Message ?? "Unknown error";
            System.Windows.MessageBox.Show($"Ein unerwarteter Fehler ist aufgetreten:\n{msg}", "Fehler", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch { }
    }
}