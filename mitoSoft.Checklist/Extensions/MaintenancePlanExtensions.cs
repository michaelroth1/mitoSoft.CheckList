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

    public static bool IsValid(this MaintenancePlan plan)
    {
        return plan != null
            && plan.Steps.Count > 0;
    }

    public static bool IsValidCurrentStep(this MaintenancePlan plan, int currentIndex)
    {
        return plan.IsValid()
            && currentIndex >= 0
            && currentIndex < plan.Steps.Count;
    }

    public static bool IsValidTaskIndex(this MaintenancePlan plan, int currentIndex, int taskIndex)
    {
        return plan.IsValidCurrentStep(currentIndex)
            && taskIndex >= 0
            && taskIndex < plan.Steps[currentIndex].Tasks.Count;
    }
}