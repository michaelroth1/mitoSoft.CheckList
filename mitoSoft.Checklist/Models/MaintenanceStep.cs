namespace TobisChecklist.Models;

public class MaintenanceStep
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<MaintenanceTask> Tasks { get; set; } = new();
}