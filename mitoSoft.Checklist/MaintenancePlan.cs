using System.IO;
using System.Xml.Linq;
using TobisChecklist.Models;

namespace TobisChecklist;

public class MaintenancePlan
{
    public List<MaintenanceStep> Steps { get; set; } = [];

    public string Path { get; set; } = string.Empty;

    public string SourcePath { get; private set; } = string.Empty;

    public string Name { get; private set; } = "Unnamed Plan";

    public MaintenancePlan(string path)
    { 
        this.Path = path;
        this.SourcePath = path;
        this.LoadFromXml();
    }

    // Overwrites the original template XML with current task states and photo attributes
    public void SaveToTemplate()
    {
        this.TrySaveToTemplateAs(this.Path);
    }

    public void TrySaveToTemplateAs(string saveAs)
    {
        try
        {
            this.SaveToTemplateAs(saveAs);
        }
        catch (Exception ex)
        {
            // Log the error, show message to user, etc.
            throw new InvalidOperationException($"Fehler beim Speichern des Plans: {ex.Message}");
        }
    }

    // Save current state into a new template file selected by user
    public void SaveToTemplateAs(string saveAs)
    {
        if (string.IsNullOrEmpty(this.SourcePath)) throw new InvalidOperationException("No source path available");

        var doc = XDocument.Load(this.SourcePath);
        var root = doc.Root;
        if (root == null) throw new InvalidOperationException("Invalid plan XML");

        var stepEls = root.Elements("Step").ToList();
        for (int si = 0; si < stepEls.Count && si < Steps.Count; si++)
        {
            var stepEl = stepEls[si];
            var taskEls = stepEl.Elements("Task").ToList();
            var tasks = Steps[si].Tasks;
            for (int ti = 0; ti < taskEls.Count && ti < tasks.Count; ti++)
            {
                var tEl = taskEls[ti];
                var task = tasks[ti];
                tEl.SetAttributeValue("done", task.Done.ToString().ToLowerInvariant());

                if (!string.IsNullOrEmpty(task.PhotoPath))
                {
                    tEl.SetAttributeValue("photo", task.PhotoPath);
                }
                else
                {
                    var a = tEl.Attribute("photo");
                    if (a != null) a.Remove();
                }

                if (!string.IsNullOrEmpty(task.UserInput))
                {
                    tEl.SetAttributeValue("userInput", task.UserInput);
                }
                else
                {
                    var a = tEl.Attribute("userInput");
                    if (a != null) a.Remove();
                }
            }
        }

        var dir = System.IO.Path.GetDirectoryName(saveAs);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        doc.Save(saveAs);

        this.Path = saveAs;
        this.SourcePath = saveAs;
    }

    public void LoadFromXml()
    {
        this.Steps.Clear();

        XDocument doc = XDocument.Load(this.Path);
        var root = doc.Root;

        if (root != null)
        {
            var metadata = root.Element("Metadata");
            var nameEl = metadata?.Element("Name");
            Name = string.IsNullOrWhiteSpace(nameEl?.Value) ? "Unnamed Plan" : nameEl.Value.Trim();
        }

        foreach (var stepEl in root.Elements("Step"))
        {
            var s = new MaintenanceStep
            {
                Title = (string?)stepEl.Element("Title") ?? string.Empty,
                Description = (string?)stepEl.Element("Description") ?? string.Empty,
                Tasks = stepEl.Elements("Task").Select(t => new MaintenanceTask
                {
                    Text = (string?)t ?? string.Empty,
                    Done = ((string?)t.Attribute("done") == "true"),
                    Type = ((string?)t.Attribute("type")) ?? "Check",
                    PhotoPath = ((string?)t.Attribute("photo")),
                    Mode = ((string?)t.Attribute("mode")) ?? "Optional",
                    UserInput = ((string?)t.Attribute("userInput"))
                }).ToList()
            };

            this.Steps.Add(s);
        }
    }
}