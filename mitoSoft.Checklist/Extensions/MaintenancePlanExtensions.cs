using mitoSoft.Checklist.Helpers;
using System.IO;

namespace mitoSoft.Checklist.Extensions;

public static class MaintenancePlanExtensions
{
    public static string GetDefaultFileName(this MaintenancePlan plan)
    {
        var planName = plan.Name.Replace(" ", "_");
        var datePart = DateTime.Now.ToString("yyyy-MM-dd");
        var defaultName = $"{datePart}_{planName}.xml";
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            defaultName = defaultName.Replace(c, '_');
        }

        return defaultName;
    }
}