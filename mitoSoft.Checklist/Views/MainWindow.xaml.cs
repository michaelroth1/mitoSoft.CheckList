using mitoSoft.Checklist.Extensions;
using mitoSoft.Checklist.Helpers;
using mitoSoft.Checklist.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace mitoSoft.Checklist;

public partial class MainWindow : Window
{
    private MaintenancePlan? _plan;
    private int _currentIndex = -1;
    private readonly UiModeManager? _uiModeManager;

    public MainWindow()
    {
        InitializeComponent();

        // Only load runtime resources when not in design mode
        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            _uiModeManager = new UiModeManager(this);
            _uiModeManager.ApplyMode(false);
            ClearWizard();
            UpdateButtonState();
        }
    }

    #region Button Click Event Handlers

    private void ToggleMode_Click(object sender, RoutedEventArgs e)
    {
        _uiModeManager?.ToggleMode();

        // Re-render tasks with appropriate sizing
        if (_plan?.IsValidCurrentStep(_currentIndex) == true)
        {
            RenderTasksForCurrentStep();
        }
    }

    private void LoadTemplate_Clicked(object sender, RoutedEventArgs e)
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var vorlagenDirectory = new DirectoryInfo(Path.Combine(root, "Wartungsvorlagen"));

        var file = FileDialogService.OpenXmlFile(vorlagenDirectory);

        if (file == null) return;

        TryLoadPlan(file);
        _plan!.Path = string.Empty;
    }

    private void Load_Clicked(object sender, RoutedEventArgs e)
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var wartungenDirectory = new DirectoryInfo(Path.Combine(root, "Wartungen"));

        var file = FileDialogService.OpenXmlFile(wartungenDirectory);

        if (file == null) return;

        TryLoadPlan(file);
    }

    private void Save_Clicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_plan!.Path))
        {
            SaveAs_Clicked(sender, e);
            return;
        }

        _plan!.SaveToTemplate();

        lblStatus.Text = $"Plan erfolgreich gespeichert. ({DateTime.Now})";
    }

    private void SaveAs_Clicked(object sender, RoutedEventArgs e)
    {
        var documentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var wartungenDirectory = new DirectoryInfo(Path.Combine(documentsRoot, "Wartungen"));
        var defaultFileName = _plan!.GetDefaultFileName().Name;

        var filePath = FileDialogService.SaveXmlFile(wartungenDirectory, defaultFileName);

        if (filePath == null) return;

        _plan!.TrySaveToTemplateAs(filePath);

        lblStatus.Text = $"Plan erfolgreich gespeichert. ({DateTime.Now})";
    }

    private void ExportPdf_Clicked(object sender, RoutedEventArgs e)
    {
        var defaultFolderName = _plan!
            .GetDefaultFileName()
            .GetFileNameWithoutExtension();

        var exportFolder = DirectoryService.SelectAndPrepareExportDirectory(defaultFolderName);

        if (exportFolder == null) return;

        TryExport(exportFolder);
    }

    private void Back_Clicked(object sender, RoutedEventArgs e)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            RenderCurrentStep();
        }
    }

    private void Next_Clicked(object sender, RoutedEventArgs e)
    {
        if (_currentIndex < _plan!.Steps.Count - 1)
        {
            _currentIndex++;
            RenderCurrentStep();
        }
        else
        {
            MessageBoxService.ShowInfo("End of maintenance plan.");
        }
    }

    #endregion

    #region UI State Management

    private void ClearWizard()
    {
        _currentIndex = -1;
        _plan = null;
        lblPlanTitle.Text = "Kein Plan geladen...";
        lblStepDescription.Text = string.Empty;
        spTasks.Children.Clear();
        lblStatus.Text = string.Empty;

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool hasPlan = _plan != null;
        bool hasValidStep = _plan?.IsValidCurrentStep(_currentIndex) ?? false;

        // Buttons requiring a loaded plan
        btnSave.IsEnabled = hasPlan;
        btnSaveAs.IsEnabled = hasPlan;
        btnExportPdf.IsEnabled = hasPlan;

        if (!hasPlan)
        {
            spTasks.Children.Clear();
        }

        // Navigation buttons
        btnBack.IsEnabled = hasValidStep && _currentIndex >= 1;

        btnNext.IsEnabled = hasValidStep && _plan!.Steps[_currentIndex].Tasks.All(t => t.Done);
    }

    #endregion

    #region Rendering

    private void RenderCurrentStep()
    {
        if (_plan == null || !_plan.IsValidCurrentStep(_currentIndex))
        {
            return;
        }

        var currentStep = _plan.Steps[_currentIndex];
        groupBoxSteps.Header = $"Wartungsschritt: {currentStep.Title}";
        lblStepDescription.Text = currentStep.Description;

        RenderTasksForCurrentStep();

        lblStepIndex.Text = $"Step {_currentIndex + 1} of {_plan.Steps.Count}";

        UpdateButtonState();
    }

    private void RenderTasksForCurrentStep()
    {
        spTasks.Children.Clear();

        if (_plan == null
            || !_plan.IsValidCurrentStep(_currentIndex))
        {
            return;
        }

        var currentStep = _plan.Steps[_currentIndex];
        for (int i = 0; i < currentStep.Tasks.Count; i++)
        {
            var task = currentStep.Tasks[i];
            var taskElement = task.Type?.ToLower() switch
            {
                "text" => _uiModeManager!.CreateTextInputTask(task, i, TextInput_Changed),
                "zahl" => _uiModeManager!.CreateNumberInputTask(task, i, TextInput_Changed, NumberBox_PreviewTextInput),
                "photo" => _uiModeManager!.CreatePhotoTask(task, i, TaskCheck_Changed, TaskCheck_Changed, LinkLabel_Clicked),
                "check" => _uiModeManager!.CreateCheckboxTask(task, i, TaskCheck_Changed, TaskCheck_Changed),
                _ => throw new InvalidOperationException($"Unknown task type: {task.Type}")
            };
            spTasks.Children.Add(taskElement);
        }
    }

    #endregion

    #region Task Event Handlers

    private void NumberBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        if (sender is not WpfTextBox textBox)
        {
            e.Handled = true;
            return;
        }

        if (!e.Text.All(c => char.IsDigit(c) || c == ','))
        {
            e.Handled = true;
            return;
        }

        // Simuliere den Text nach der Eingabe
        string text = textBox.Text.Insert(textBox.SelectionStart, e.Text);

        // Leerer Text ist OK (Benutzer kann löschen)
        if (string.IsNullOrWhiteSpace(text))
        {
            e.Handled = false;
            return;
        }

        // Blockiere Eingabe, wenn keine gültige Zahl
        e.Handled = !double.TryParse(text, out _);
    }

    private void TextInput_Changed(object sender, TextChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox tb && tb.Tag is int idx)
        {
            if (_plan == null || !_plan.IsValidTaskIndex(_currentIndex, idx)) return;

            var task = _plan.Steps[_currentIndex].Tasks[idx];
            task.UserInput = tb.Text;
            task.Done = !string.IsNullOrWhiteSpace(tb.Text);
        }
    }

    private void LinkLabel_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is TextBlock link && link.Tag is int taskIndex)
        {
            if (_plan == null || !_plan.IsValidTaskIndex(_currentIndex, taskIndex)) return;

            var task = _plan.Steps[_currentIndex].Tasks[taskIndex];
            var file = new FileInfo(task.PhotoPath!);
            file.TryShowPhoto();
        }
    }

    private void TaskCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is WpfCheckBox chk && chk.Tag is int idx)
        {
            if (_plan == null || !_plan.IsValidTaskIndex(_currentIndex, idx)) return;

            var task = _plan.Steps[_currentIndex].Tasks[idx];

            // If Photo task and user checked it, start photo flow
            if (chk.IsChecked == true
                && task.Type?.Equals("Photo", StringComparison.OrdinalIgnoreCase) == true)
            {
                chk.Checked -= TaskCheck_Changed;
                chk.Unchecked -= TaskCheck_Changed;
                chk.IsChecked = false;
                chk.Checked += TaskCheck_Changed;
                chk.Unchecked += TaskCheck_Changed;
                AttachPhotoToTask(idx);
                return;
            }

            task.Done = chk.IsChecked == true;

            if (chk.IsChecked == false)
            {
                task.PhotoPath = null;

                // Remove photo link from UI
                if (chk.Parent is StackPanel panel)
                {
                    TextBlock? linkToRemove = null;
                    foreach (var child in panel.Children)
                    {
                        if (child is TextBlock tb && tb.Tag is int ltag && ltag == idx)
                        {
                            linkToRemove = tb;
                            break;
                        }
                    }
                    if (linkToRemove != null)
                    {
                        panel.Children.Remove(linkToRemove);
                    }
                }
            }

            UpdateButtonState();
        }
    }

    #endregion

    #region Photo Management

    private void AttachPhotoToTask(int taskIndex)
    {
        if (_plan == null || !_plan.IsValidTaskIndex(_currentIndex, taskIndex)) return;

        var selectedTask = _plan.Steps[_currentIndex].Tasks[taskIndex];
        var photoPath = CameraService.CapturePhotoFromCamera(status => lblStatus.Text = status);

        // Bring the application back to foreground after photo capture
        this.Activate();
        this.Topmost = true;
        this.Topmost = false;
        this.Focus();

        if (!string.IsNullOrEmpty(photoPath))
        {
            TryLinkPhotoWithTask(taskIndex, selectedTask, photoPath);
        }
    }

    private void TryLinkPhotoWithTask(int taskIndex, MaintenanceTask task, string photoPath)
    {
        try
        {
            task.PhotoPath = photoPath;
            task.Done = true;

            UpdatePhotoTaskUI(taskIndex, photoPath);

            UpdateButtonState();

            lblStatus.Text = "Foto vermerkt (Originalpfad wird beibehalten).";
        }
        catch (Exception ex)
        {
            throw new Exception($"Fehler beim Verlinken des Fotos: {ex.Message}");
        }
    }

    private void UpdatePhotoTaskUI(int taskIndex, string photoPath)
    {
        foreach (var panel in spTasks.Children.OfType<StackPanel>())
        {
            WpfCheckBox? checkbox = null;
            TextBlock? photoLink = null;

            foreach (var child in panel.Children)
            {
                if (child is WpfCheckBox cb && cb.Tag is int tag && tag == taskIndex)
                {
                    checkbox = cb;
                }
                if (child is TextBlock tb && tb.Tag is int ltag && ltag == taskIndex)
                {
                    photoLink = tb;
                }
            }

            if (checkbox != null)
            {
                checkbox.Checked -= TaskCheck_Changed;
                checkbox.Unchecked -= TaskCheck_Changed;
                checkbox.IsChecked = true;
                checkbox.Checked += TaskCheck_Changed;
                checkbox.Unchecked += TaskCheck_Changed;

                if (photoLink == null)
                {
                    var link = _uiModeManager!.CreatePhotoLinkLabel(taskIndex, photoPath, LinkLabel_Clicked);
                    panel.Children.Add(link);
                }
            }
        }
    }

    #endregion

    #region Plan Loading

    private void TryLoadPlan(FileInfo file)
    {
        try
        {
            _plan = new MaintenancePlan(file.FullName);

            lblPlanTitle.Text = _plan.Name;

            if (!_plan.IsValid())
            {
                throw new InvalidOperationException("Invalid Maintenance plan - no steps");
            }

            _currentIndex = 0;

            RenderCurrentStep();

            lblStatus.Text = "Erfolgreich geladen.";
        }
        catch (Exception ex)
        {
            ClearWizard();
            throw new InvalidOperationException($"Failed to load XML: {ex.Message}");
        }
        finally
        {
            UpdateButtonState();
        }
    }

    #endregion

    #region Export

    private void TryExport(string exportFolder)
    {
        try
        {
            var photoDir = Path.Combine(exportFolder, "Photos");

            this._plan!.CopyPhotos(photoDir);

            var pdfPath = Path.Combine(exportFolder, "Wartungsbericht.pdf");
            var exporter = new PdfExporter(pdfPath);
            exporter.Export(_plan!, "Wartungsbericht");

            MessageBoxService.ShowInfo("Export abgeschlossen.", "Export");
        }
        catch (Exception ex)
        {
            throw new Exception($"Fehler beim Export: {ex.Message}");
        }
    }

    #endregion
}