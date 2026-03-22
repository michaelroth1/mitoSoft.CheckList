using mitoSoft.Checklist.Helpers;
using System.IO;

namespace mitoSoft.Checklist.Extensions;

public static class MaintenancePlanExtensions
{
    public static FileInfo GetDefaultFileName(this MaintenancePlan plan)
    {
        var planName = plan.Name.Replace(" ", "_");

        return planName.GetDefaultFileName();
    }
}