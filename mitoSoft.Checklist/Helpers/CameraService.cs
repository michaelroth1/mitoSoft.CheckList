using System.IO;
using WpfApplication = System.Windows.Application;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace mitoSoft.Checklist.Helpers;

public static class CameraService
{
    private const int PhotoDetectionTimeoutSeconds = 30;
    private const string ImageFileFilter = "Image files|*.jpg;*.jpeg;*.png;*.bmp|All files|*.*";

    public static string? CapturePhotoFromCamera(Action<string> updateStatus)
    {
        try
        {
            var picRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var candidateDirs = GetCandidatePhotoDirectories(picRoot);
            var existingFiles = GetExistingFiles(candidateDirs);
            var startTime = DateTime.UtcNow;

            var cameraProcess = StartCameraApp();
            updateStatus("Kamera gestartet — Foto aufnehmen. Warte auf neues Foto...");

            var newPhotoPath = WaitForNewPhoto(candidateDirs, existingFiles, startTime, cameraProcess, updateStatus);

            if (newPhotoPath == null)
            {
                updateStatus("Kein neues Foto automatisch gefunden — bitte Datei wählen.");
                return SelectPhotoFromFile(picRoot, updateStatus);
            }

            updateStatus("Neues Foto automatisch gefunden.");
            return newPhotoPath;
        }
        catch (Exception ex)
        {
            updateStatus($"Fehler: {ex.Message}");
            return null;
        }
    }

    public static string? SelectPhotoFromFile(string? initialDirectory = null, Action<string>? updateStatus = null)
    {
        var picRoot = initialDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        
        var ofd = new WpfOpenFileDialog
        {
            InitialDirectory = picRoot,
            Filter = ImageFileFilter
        };

        if (ofd.ShowDialog() == true)
        {
            return ofd.FileName;
        }

        updateStatus?.Invoke(string.Empty);
        return null;
    }

    private static List<string> GetCandidatePhotoDirectories(string picRoot)
    {
        return
        [
            Path.Combine(picRoot, "Camera Roll"),
            Path.Combine(picRoot, "Saved Pictures"),
            picRoot
        ];
    }

    private static HashSet<string> GetExistingFiles(List<string> directories)
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in directories)
        {
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    existing.Add(file);
                }
            }
        }
        return existing;
    }

    private static System.Diagnostics.Process? StartCameraApp()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("microsoft.windows.camera:") { UseShellExecute = true };
            return System.Diagnostics.Process.Start(psi);
        }
        catch
        {
            return null;
        }
    }

    private static string? WaitForNewPhoto(List<string> directories, HashSet<string> existingFiles, DateTime startTime, System.Diagnostics.Process? cameraProcess, Action<string> updateStatus)
    {
        var timeout = TimeSpan.FromSeconds(PhotoDetectionTimeoutSeconds);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (!IsCameraStillRunning(cameraProcess))
            {
                updateStatus("Kamera-App wurde geschlossen. Vorgang abgebrochen.");
                return null;
            }

            var newPhoto = FindNewPhoto(directories, existingFiles, startTime);
            if (newPhoto != null)
            {
                return newPhoto;
            }

            System.Threading.Thread.Sleep(500);
            WpfApplication.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
        }

        return null;
    }

    private static bool IsCameraStillRunning(System.Diagnostics.Process? cameraProcess)
    {
        if (cameraProcess != null)
        {
            try
            {
                return !cameraProcess.HasExited;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            try
            {
                var cameraProcesses = System.Diagnostics.Process.GetProcessesByName("WindowsCamera");
                return cameraProcesses.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }

    private static string? FindNewPhoto(List<string> directories, HashSet<string> existingFiles, DateTime startTime)
    {
        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory)) continue;

            foreach (var file in Directory.GetFiles(directory))
            {
                if (existingFiles.Contains(file)) continue;

                try
                {
                    var writeTime = File.GetLastWriteTimeUtc(file);
                    if (writeTime >= startTime.AddSeconds(-2))
                    {
                        return file;
                    }
                }
                catch { }
            }
        }
        return null;
    }
}