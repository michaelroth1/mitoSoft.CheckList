using System.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
// annotations not used - write file:// links as text so PDF viewers can detect them
using mitoSoft.Checklist.Models;

namespace mitoSoft.Checklist.Helpers;

internal class PdfExporter(string filePath)
{
    private readonly string _filePath = filePath;

    public void Export(MaintenancePlan plan, string title)
    {
        using var doc = new PdfDocument();
        var page = doc.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        var gfx = XGraphics.FromPdfPage(page);
        var fontTitle = new XFont("Verdana", 16, XFontStyle.Bold);
        var fontNormal = new XFont("Verdana", 10, XFontStyle.Regular);

        double y = 40;
        gfx.DrawString(title, fontTitle, XBrushes.Black, new XRect(40, y, page.Width - 80, 30), XStringFormats.TopLeft);
        y += 40;

        for (int i = 0; i < plan.Steps.Count; i++)
        {
            var step = plan.Steps[i];
            gfx.DrawString($"Schritt {i + 1}: {step.Title}", fontNormal, XBrushes.Black, new XRect(40, y, page.Width - 80, 20), XStringFormats.TopLeft);
            y += 18;
            gfx.DrawString(step.Description, fontNormal, XBrushes.DarkGray, new XRect(60, y, page.Width - 100, 40), XStringFormats.TopLeft);
            y += 30;

            foreach (var task in step.Tasks)
            {
                var mark = task.Done ? "[X]" : "[ ]";
                gfx.DrawString($"{mark} {task.Text}", fontNormal, XBrushes.Black, new XRect(80, y, page.Width - 120, 20), XStringFormats.TopLeft);
                y += 16;
                // if a photo is attached, render filename and add a clickable link to the file
                if (!string.IsNullOrEmpty(task.PhotoPath))
                {
                    var fileName = Path.GetFileName(task.PhotoPath);
                    // render a file:// URI as text - many PDF viewers will make this clickable
                    var uriText = "file://" + task.PhotoPath.Replace('\\', '/');
                    var linkText = $"(Foto: {fileName}) {uriText}";
                    var linkRect = new XRect(100, y - 4, page.Width - 160, 16);
                    gfx.DrawString(linkText, fontNormal, XBrushes.Blue, linkRect, XStringFormats.TopLeft);

                    y += 14;
                }
                if (y > page.Height - 80)
                {
                    page = doc.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            y += 8;
        }

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        using var st = File.OpenWrite(_filePath);
        doc.Save(st);
    }
}
