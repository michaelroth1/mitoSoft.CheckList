using mitoSoft.Checklist.Helpers;
using System.IO;

namespace mitoSoft.Checklist.Extensions;

public static class MaintenancePlanExtensions
{
    public static FileInfo GetDefaultFileName(this MaintenancePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var planName = plan.Name.Replace(" ", "_");

        return planName.GetDefaultFileName();
    }

    public static void CopyPhotos(this MaintenancePlan plan, string directory)
    {
        ArgumentNullException.ThrowIfNull(plan);

        Directory.CreateDirectory(directory);

        foreach (var step in plan.Steps)
        {
            foreach (var task in step.Tasks)
            {
                if (!string.IsNullOrEmpty(task.PhotoPath) && File.Exists(task.PhotoPath))
                {
                    try
                    {
                        var dest = Path.Combine(directory, Path.GetFileName(task.PhotoPath));
                        File.Copy(task.PhotoPath, dest, true);
                    }
                    catch { }
                }
            }
        }
    }
}