using System.IO;

namespace mitoSoft.Checklist.Extensions;

public static class FileInfoExtensions
{
    public static FileInfo GetValidFileName(this FileInfo file)
    {
        var dir = file.DirectoryName ?? string.Empty;
        var fileName = file.Name;
        fileName = fileName.Replace(" ", "_");

        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        return new FileInfo(Path.Combine(dir, fileName));
    }

    public static string GetFileNameWithoutExtension(this FileInfo file)
    {
        return Path.GetFileNameWithoutExtension(file.Name);
    }

    public static FileInfo GetDefaultFileName(this string name)
    {
        var datePart = DateTime.Now.ToString("yyyy-MM-dd");
        var defaultName = new FileInfo($"{datePart}_{name}.xml");

        return defaultName.GetValidFileName();
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