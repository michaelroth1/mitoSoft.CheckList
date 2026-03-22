namespace mitoSoft.Checklist.Models;

public class MaintenanceTask
{
    public string Text { get; set; } = string.Empty;
    public bool Done { get; set; } = false;
    public string? PhotoPath { get; set; }
    public string Type { get; set; } = "Check"; // Check, Photo, Text, or Zahl
    public string Mode { get; set; } = "Optional";
    public string? UserInput { get; set; }
}