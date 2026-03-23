using System.IO;
using WpfMessageBox = System.Windows.MessageBox;

namespace mitoSoft.Checklist.Helpers;

public class ExportDirectoryService
{
    public static string? SelectAndPrepareExportDirectory(string defaultFolderName)
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var exportFolder = Path.Combine(root, "Wartungen", defaultFolderName);

        Directory.CreateDirectory(exportFolder);

        var selectedFolder = ShowFolderBrowserDialog(exportFolder);
        if (string.IsNullOrEmpty(selectedFolder))
        {
            return null;
        }

        if (!ConfirmAndClearDirectory(selectedFolder))
        {
            return null;
        }

        PrepareDirectory(selectedFolder);
        return selectedFolder;
    }

    private static string? ShowFolderBrowserDialog(string initialDirectory)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Wähle Zielordner für Export (Fotos und Bericht werden dort abgelegt)",
            SelectedPath = initialDirectory
        };

        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK 
            ? dialog.SelectedPath 
            : null;
    }

    private static bool ConfirmAndClearDirectory(string directory)
    {
        if (!Directory.Exists(directory) || !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            return true;
        }

        var result = WpfMessageBox.Show(
            "Der ausgewählte Ordner ist nicht leer. Alle vorhandenen Dateien in diesem Ordner werden gelöscht. Fortfahren?",
            "Ordner nicht leer",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        return result == System.Windows.MessageBoxResult.Yes;
    }

    private static void PrepareDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
            System.Threading.Thread.Sleep(500);
        }

        Directory.CreateDirectory(directory);
    }
}
