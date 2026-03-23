using System.IO;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace mitoSoft.Checklist.Helpers;

public static class FileDialogService
{
    private const string XmlFileFilter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

    public static FileInfo? OpenXmlFile(DirectoryInfo initialDirectory)
    {
        Directory.CreateDirectory(initialDirectory.FullName);

        var openFileDialog = new WpfOpenFileDialog
        {
            Filter = XmlFileFilter,
            InitialDirectory = initialDirectory.FullName
        };

        return openFileDialog.ShowDialog() == true 
            ? new FileInfo(openFileDialog.FileName) 
            : null;
    }

    public static string? SaveXmlFile(DirectoryInfo initialDirectory, string defaultFileName)
    {
        Directory.CreateDirectory(initialDirectory.FullName);

        var saveFileDialog = new WpfSaveFileDialog
        {
            Filter = XmlFileFilter,
            InitialDirectory = initialDirectory.FullName,
            FileName = defaultFileName
        };

        return saveFileDialog.ShowDialog() == true 
            ? saveFileDialog.FileName 
            : null;
    }
}