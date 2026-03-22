using System.IO;

namespace TobisChecklist.Extensions;

public static class FileInfoExtensions
{
    public static FileInfo GetDefaultFileName(this string name)
    {
        name = name.Replace(" ", "_");
        var datePart = DateTime.Now.ToString("yyyy-MM-dd");
        var defaultName = $"{datePart}_{name}.xml";
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            defaultName = defaultName.Replace(c, '_');
        }

        return new FileInfo(defaultName);
    }

    public static void TryShowPhoto(this FileInfo file)
    {
        var path = file.FullName;

        if (string.IsNullOrEmpty(path))
        {
            throw new InvalidOperationException("Beim Abspeichern des Fotos ist ein Fehler aufgetreten");
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Foto nicht gefunden: {path}");
        }

        var psi = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true };
        System.Diagnostics.Process.Start(psi);
    }
}