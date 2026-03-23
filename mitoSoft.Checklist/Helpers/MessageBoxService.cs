using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;

namespace mitoSoft.Checklist.Helpers;

public static class MessageBoxService
{
    public static void ShowError(string message, string title = "Fehler")
    {
        WpfMessageBox.Show(message, title, WpfMessageBoxButton.OK, WpfMessageBoxImage.Error);
    }

    public static void ShowInfo(string message, string title = "Information")
    {
        WpfMessageBox.Show(message, title, WpfMessageBoxButton.OK, WpfMessageBoxImage.Information);
    }

    public static void ShowWarning(string message, string title = "Warnung")
    {
        WpfMessageBox.Show(message, title, WpfMessageBoxButton.OK, WpfMessageBoxImage.Warning);
    }

    public static bool ShowConfirmation(string message, string title = "Bestätigung")
    {
        var result = WpfMessageBox.Show(message, title, WpfMessageBoxButton.YesNo, WpfMessageBoxImage.Question);
        return result == WpfMessageBoxResult.Yes;
    }

    public static WpfMessageBoxResult ShowQuestion(string message, string title = "Frage")
    {
        return WpfMessageBox.Show(message, title, WpfMessageBoxButton.YesNoCancel, WpfMessageBoxImage.Question);
    }
}
