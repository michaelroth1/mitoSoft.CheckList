using mitoSoft.Checklist.Helpers;
using mitoSoft.Checklist.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using mitoSoft.Checklist.Extensions;
using WpfApplication = System.Windows.Application;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfMessageBox = System.Windows.MessageBox;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;
using mitoSoft.Checklist.Converters;

namespace mitoSoft.Checklist;

public partial class MainWindow : Window
{
    private MaintenancePlan? _plan;
    private int _currentIndex = -1;
    private bool _isTabletMode = false;
    private UiModeManager? _uiModeManager;

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

    private void UpdateButtonState()
    {
        bool hasPlan = _plan != null;
        bool hasValidStep = hasPlan && _currentIndex >= 0 && _currentIndex < _plan!.Steps.Count;

        // Buttons requiring a loaded plan
        btnSave.IsEnabled = hasPlan;
        btnSaveAs.IsEnabled = hasPlan;
        btnAttachPhoto.IsEnabled = hasPlan;
        btnExportPdf.IsEnabled = hasPlan;

        if (!hasPlan)
        {
            spTasks.Children.Clear();
        }

        // Navigation buttons
        btnBack.IsEnabled = hasValidStep && _currentIndex >= 1;

        btnNext.IsEnabled = hasValidStep && _plan!.Steps[_currentIndex].Tasks.All(t => t.Done);
    }

    private void ToggleMode_Click(object sender, RoutedEventArgs e)
    {
        _isTabletMode = !_isTabletMode;
        _uiModeManager?.ApplyMode(_isTabletMode);

        // Re-render tasks with appropriate sizing
        if (_plan != null && _currentIndex >= 0)
        {
            RenderTasksForCurrentStep();
        }
    }

    private void RenderTasksForCurrentStep()
    {
        spTasks.Children.Clear();
        if (_plan == null
            || _currentIndex < 0
            || _currentIndex >= _plan.Steps.Count)
        {
            return;
        }

        var currentStep = _plan.Steps[_currentIndex];
        for (int i = 0; i < currentStep.Tasks.Count; i++)
        {
            var task = currentStep.Tasks[i];
            var taskElement = task.Type?.ToLower() switch
            {
                "text" => CreateTextInputTask(task, i),
                "zahl" => CreateNumberInputTask(task, i),
                "photo" => CreatePhotoTask(i, task),
                _ => CreateCheckboxTask(task, i)
            };
            spTasks.Children.Add(taskElement);
        }
    }

    private StackPanel CreateTextInputTask(MaintenanceTask task, int index)
    {
        var panel = new StackPanel
        {
            Orientation = WpfOrientation.Vertical,
            Margin = new Thickness(0, 5, 0, 15)
        };

        var label = new TextBlock
        {
            Text = task.Text,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 5)
        };
        panel.Children.Add(label);

        var textBox = new System.Windows.Controls.TextBox
        {
            Text = task.UserInput ?? string.Empty,
            Tag = index,
            FontSize = _isTabletMode ? 16 : 14,
            MinHeight = 30,
            Padding = _isTabletMode ? new Thickness(10) : new Thickness(5),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinLines = 2
        };
        textBox.TextChanged += TextInput_Changed;
        panel.Children.Add(textBox);

        return panel;
    }

    private StackPanel CreateNumberInputTask(MaintenanceTask task, int index)
    {
        var panel = new StackPanel
        {
            Orientation = WpfOrientation.Vertical,
            Margin = new Thickness(0, 5, 0, 15)
        };

        var label = new TextBlock
        {
            Text = task.Text,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 5)
        };
        panel.Children.Add(label);

        var numberBox = new System.Windows.Controls.TextBox
        {
            Text = task.UserInput ?? string.Empty,
            Tag = index,
            FontSize = _isTabletMode ? 16 : 14,
            MinHeight = 30,
            Padding = _isTabletMode ? new Thickness(10) : new Thickness(5),
            AcceptsReturn = false,
            TextWrapping = TextWrapping.NoWrap,
            InputScope = new System.Windows.Input.InputScope
            {
                Names = { new System.Windows.Input.InputScopeName(System.Windows.Input.InputScopeNameValue.Number) }
            }
        };
        numberBox.PreviewTextInput += NumberBox_PreviewTextInput;
        numberBox.TextChanged += TextInput_Changed;
        panel.Children.Add(numberBox);

        return panel;
    }

    private StackPanel CreateCheckboxTask(MaintenanceTask task, int index)
    {
        var panel = new StackPanel
        {
            Orientation = WpfOrientation.Horizontal,
            Margin = _isTabletMode ? new Thickness(0, 10, 0, 10) : new Thickness(0, 5, 0, 5)
        };

        var checkbox = _uiModeManager!.CreateTaskCheckbox(index, task.Text, task.Done, TaskCheck_Changed, TaskCheck_Changed);
        panel.Children.Add(checkbox);

        return panel;
    }

    private StackPanel CreatePhotoTask(int index, MaintenanceTask task)
    {
        var panel = new StackPanel
        {
            Orientation = WpfOrientation.Horizontal,
            Margin = _isTabletMode ? new Thickness(0, 10, 0, 10) : new Thickness(0, 5, 0, 5)
        };

        var checkbox = _uiModeManager!.CreateTaskCheckbox(index, task.Text, task.Done, TaskCheck_Changed, TaskCheck_Changed);
        panel.Children.Add(checkbox);

        if (!string.IsNullOrEmpty(task.PhotoPath))
        {
            var link = _uiModeManager!.CreatePhotoLink(index, task.PhotoPath, Link_Click);
            panel.Children.Add(link);
        }

        return panel;
    }

    private void NumberBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !IsNumericInput(e.Text);
    }

    private static bool IsNumericInput(string text)
    {
        return text.All(c => char.IsDigit(c) || c == ',' || c == '.');
    }

    private void TextInput_Changed(object sender, TextChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox tb && tb.Tag is int idx)
        {
            var cur = _plan!.Steps[_currentIndex];
            if (idx < 0 || idx >= cur.Tasks.Count) return;

            var task = cur.Tasks[idx];
            task.UserInput = tb.Text;
            task.Done = !string.IsNullOrWhiteSpace(tb.Text);
        }
    }
        
    private void Link_Click(int taskIndex)
    {
        var task = _plan!.Steps[_currentIndex].Tasks[taskIndex];

        var file = new FileInfo(task.PhotoPath!);
        file.TryShowPhoto();
    }

    private void TaskCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is WpfCheckBox chk && chk.Tag is int idx)
        {
            var cur = _plan!.Steps[_currentIndex];
            if (idx < 0 || idx >= cur.Tasks.Count) return;
            var task = cur.Tasks[idx];

            // If Photo task and user checked it, start photo flow
            if (chk.IsChecked == true
                && task.Type?.Equals("Photo", StringComparison.OrdinalIgnoreCase) == true)
            {
                chk.Checked -= TaskCheck_Changed;
                chk.Unchecked -= TaskCheck_Changed;
                chk.IsChecked = false;
                chk.Checked += TaskCheck_Changed;
                chk.Unchecked += TaskCheck_Changed;
                AttachPhotoToTask(idx, requireCamera: true);
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

    private void AttachPhotoToTask(int taskIndex, bool requireCamera)
    {
        if (_currentIndex < 0 || _currentIndex >= _plan!.Steps.Count) return;

        var currentStep = _plan.Steps[_currentIndex];
        if (taskIndex < 0 || taskIndex >= currentStep.Tasks.Count) return;

        var selectedTask = currentStep.Tasks[taskIndex];

        var planName = "UnnamedPlan";
        if (!string.IsNullOrEmpty(_plan.Path))
        {
            planName = _plan.Path
                .ToFileInfo()
                .GetFileNameWithoutExtension();
        }
        else if (!string.IsNullOrEmpty(currentStep.Title))
        {
            planName = currentStep.Title.Replace(' ', '_');
        }

        string? chosenPath = null;

        try
        {
            var startUtc = DateTime.UtcNow;
            var picRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var candidateDirs = new List<string>
            {
                Path.Combine(picRoot, "Camera Roll"),
                Path.Combine(picRoot, "Saved Pictures"),
                picRoot
            };
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in candidateDirs)
            {
                if (Directory.Exists(d))
                {
                    foreach (var f in Directory.GetFiles(d)) existing.Add(f);
                }
            }

            var psi = new System.Diagnostics.ProcessStartInfo("microsoft.windows.camera:") { UseShellExecute = true };
            System.Diagnostics.Process? camProc = null;
            try
            {
                camProc = System.Diagnostics.Process.Start(psi);
            }
            catch
            {
                camProc = null;
            }
            lblStatus.Text = "Kamera gestartet — Foto aufnehmen. Warte auf neues Foto...";

            string? found = null;
            var timeout = TimeSpan.FromSeconds(30);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                if (camProc != null)
                {
                    try
                    {
                        if (camProc.HasExited)
                        {
                            found = null;
                            lblStatus.Text = "Kamera-App wurde geschlossen. Vorgang abgebrochen.";
                            break;
                        }
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        var cams = System.Diagnostics.Process.GetProcessesByName("WindowsCamera");
                        if (cams.Length == 0)
                        {
                            found = null;
                            lblStatus.Text = "Kamera-App wurde nicht gefunden/geschlossen. Vorgang abgebrochen.";
                            break;
                        }
                    }
                    catch { }
                }

                foreach (var d in candidateDirs)
                {
                    if (!Directory.Exists(d)) continue;

                    foreach (var f in Directory.GetFiles(d))
                    {
                        if (existing.Contains(f)) continue;
                        try
                        {
                            var write = File.GetLastWriteTimeUtc(f);
                            if (write >= startUtc.AddSeconds(-2))
                            {
                                found = f;
                                break;
                            }
                        }
                        catch { }
                    }

                    if (found != null) break;
                }

                if (found != null) break;
                System.Threading.Thread.Sleep(500);
                WpfApplication.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
            }

            if (found == null)
            {
                lblStatus.Text = "Kein neues Foto automatisch gefunden — bitte Datei wählen.";
                var ofd = new WpfOpenFileDialog
                {
                    InitialDirectory = picRoot,
                    Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp|All files|*.*"
                };
                if (ofd.ShowDialog() != true)
                {
                    lblStatus.Text = string.Empty;
                    return;
                }
                chosenPath = ofd.FileName;
            }
            else
            {
                chosenPath = found;
                lblStatus.Text = "Neues Foto automatisch gefunden.";
            }
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show($"Kamera konnte nicht gestartet werden: {ex.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (string.IsNullOrEmpty(chosenPath)) return;

        try
        {
            selectedTask.PhotoPath = chosenPath;
            selectedTask.Done = true;

            foreach (var panel in spTasks.Children.OfType<StackPanel>())
            {
                WpfCheckBox? foundChk = null;
                TextBlock? foundLink = null;

                foreach (var child in panel.Children)
                {
                    if (child is WpfCheckBox cb && cb.Tag is int tag && tag == taskIndex)
                    {
                        foundChk = cb;
                    }
                    if (child is TextBlock tb && tb.Tag is int ltag && ltag == taskIndex)
                    {
                        foundLink = tb;
                    }
                }

                if (foundChk != null)
                {
                    foundChk.Checked -= TaskCheck_Changed;
                    foundChk.Unchecked -= TaskCheck_Changed;
                    foundChk.IsChecked = true;
                    foundChk.Checked += TaskCheck_Changed;
                    foundChk.Unchecked += TaskCheck_Changed;

                    if (foundLink == null)
                    {
                        var link = _uiModeManager!.CreatePhotoLink(taskIndex, chosenPath, Link_Click);
                        panel.Children.Add(link);
                    }
                }
            }

            lblStatus.Text = "Foto vermerkt (Originalpfad wird beibehalten).";
            UpdateButtonState();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show($"Fehler beim Vermerken des Fotos: {ex.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnLoad_Click(object sender, RoutedEventArgs e)
    {
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var root = new DirectoryInfo(Path.Combine(dir, "Wartungen"));

        var file = ChooseFile(root);

        if (file != null)
        {
            LoadPlan(file);
        }
    }

    private void btnLoadTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var root = new DirectoryInfo(Path.Combine(dir, "Wartungsvorlagen"));

        var file = ChooseFile(root);

        if (file != null)
        {
            LoadPlan(file);
            _plan!.Path = string.Empty;
        }
    }

    private FileInfo? ChooseFile(DirectoryInfo initialDirectory)
    {
        Directory.CreateDirectory(initialDirectory.FullName);

        var ofd = new WpfOpenFileDialog
        {
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            InitialDirectory = initialDirectory.FullName,
        };

        if (ofd.ShowDialog() == true)
        {
            return new FileInfo(ofd.FileName);
        }
        else
        {
            return null;
        }
    }

    private void LoadPlan(FileInfo file)
    {
        try
        {
            _plan = new MaintenancePlan(file.FullName);

            lblPlanTitle.Text = _plan.Name;

            if (_plan.Steps.Count == 0)
            {
                throw new InvalidOperationException("Invalid Maintenance plan - no steps");
            }
        }
        catch (Exception ex)
        {
            ClearWizard();
            throw new InvalidOperationException($"Failed to load XML: {ex.Message}");
        }

        _currentIndex = 0;
        RenderCurrentStep();
        UpdateButtonState();
        lblStatus.Text = "Erfolgreich geladen.";
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_plan!.Path))
        {
            btnSaveAs_Click(sender, e);
            return;
        }

        _plan.SaveToTemplate();

        lblStatus.Text = $"Plan erfolgreich gespeichert. ({DateTime.Now})";
    }

    private void btnSaveAs_Click(object sender, RoutedEventArgs e)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var folder = Path.Combine(docs, "Wartungen");

        Directory.CreateDirectory(folder);

        var sfd = new WpfSaveFileDialog
        {
            InitialDirectory = folder,
            FileName = _plan!.GetDefaultFileName().FullName,
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
        };

        if (sfd.ShowDialog() != true) return;

        _plan.TrySaveToTemplateAs(sfd.FileName);

        lblStatus.Text = $"Plan erfolgreich gespeichert. ({DateTime.Now})";
    }

    private void ClearWizard()
    {
        _currentIndex = -1;
        _plan = null;
        lblPlanTitle.Text = "Kein Plan geladen...";
        lblStepDescription.Text = string.Empty;
        spTasks.Children.Clear();
        btnNext.IsEnabled = false;
        btnBack.IsEnabled = false;
        lblStatus.Text = string.Empty;

        UpdateButtonState();
    }

    private void RenderCurrentStep()
    {
        if (_plan == null
            || _currentIndex < 0
            || _currentIndex >= _plan.Steps.Count)
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

    private void btnNext_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex < _plan!.Steps.Count - 1)
        {
            _currentIndex++;
            RenderCurrentStep();
        }
        else
        {
            WpfMessageBox.Show("End of maintenance plan.", "Finished",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            RenderCurrentStep();
        }
    }

    private void btnExportPdf_Click(object sender, RoutedEventArgs e)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var root = Path.Combine(docs, "Wartungen");
        var name = _plan!
            .GetDefaultFileName()
            .GetFileNameWithoutExtension();
        var exportFolder = Path.Combine(root, name);

        Directory.CreateDirectory(exportFolder);

        var fbd = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Wähle Zielordner für Export (Fotos und Bericht werden dort abgelegt)",
            SelectedPath = exportFolder
        };

        if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

        exportFolder = fbd.SelectedPath;

        if (Directory.Exists(exportFolder) && Directory.EnumerateFileSystemEntries(exportFolder).Any())
        {
            var res = WpfMessageBox.Show(
                "Der ausgewählte Ordner ist nicht leer. Alle vorhandenen Dateien in diesem Ordner werden gelöscht. Fortfahren?",
                "Ordner nicht leer", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;
        }

        try
        {
            Directory.Delete(exportFolder, true);
            System.Threading.Thread.Sleep(500);
            Directory.CreateDirectory(exportFolder);

            var photosDir = Path.Combine(exportFolder, "Photos");
            Directory.CreateDirectory(photosDir);

            foreach (var step in _plan.Steps)
            {
                foreach (var task in step.Tasks)
                {
                    if (!string.IsNullOrEmpty(task.PhotoPath) && File.Exists(task.PhotoPath))
                    {
                        try
                        {
                            var dest = Path.Combine(photosDir, Path.GetFileName(task.PhotoPath));
                            File.Copy(task.PhotoPath, dest, true);
                        }
                        catch { }
                    }
                }
            }

            var pdfPath = Path.Combine(exportFolder, "Wartungsbericht.pdf");
            var exporter = new PdfExporter(pdfPath);
            exporter.Export(_plan, "Wartungsbericht");

            WpfMessageBox.Show("Export abgeschlossen.", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show($"Fehler beim Export: {ex.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnAttachPhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_plan == null || _currentIndex < 0 || _currentIndex >= _plan.Steps.Count) return;

        var cur = _plan.Steps[_currentIndex];

        // Find selected checkbox
        int selIndex = -1;
        foreach (var panel in spTasks.Children.OfType<StackPanel>())
        {
            foreach (var child in panel.Children)
            {
                if (child is WpfCheckBox cb && cb.IsFocused && cb.Tag is int tag)
                {
                    selIndex = tag;
                    break;
                }
            }
            if (selIndex != -1) break;
        }

        if (selIndex < 0 || selIndex >= cur.Tasks.Count)
        {
            WpfMessageBox.Show("Bitte zuerst eine Aufgabe auswählen.", "Foto anhängen",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedTask = cur.Tasks[selIndex];

        var planName = "UnnamedPlan";
        if (!string.IsNullOrEmpty(_plan.Path))
        {
            planName = _plan.Path
                .ToFileInfo()
                .GetFileNameWithoutExtension();
        }
        else if (!string.IsNullOrEmpty(cur.Title))
        {
            planName = cur.Title.Replace(' ', '_');
        }

        var datePart = DateTime.Now.ToString("yyyy-MM-dd");
        var rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Records", datePart + "_" + planName);

        Directory.CreateDirectory(rootDir);

        var res = WpfMessageBox.Show("Foto aufnehmen mit Kamera? (Nein = Aus Datei wählen)", "Fotoquelle",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        if (res == MessageBoxResult.Cancel) return;

        string? chosenPath = null;
        if (res == MessageBoxResult.Yes)
        {
            try
            {
                var startUtc = DateTime.UtcNow;
                var picRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                var candidateDirs = new List<string>
                {
                    Path.Combine(picRoot, "Camera Roll"),
                    Path.Combine(picRoot, "Saved Pictures"),
                    picRoot
                };
                var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var d in candidateDirs)
                {
                    if (Directory.Exists(d))
                    {
                        foreach (var f in Directory.GetFiles(d)) existing.Add(f);
                    }
                }

                var psi = new System.Diagnostics.ProcessStartInfo("microsoft.windows.camera:") { UseShellExecute = true };
                System.Diagnostics.Process.Start(psi);
                lblStatus.Text = "Kamera gestartet — Foto aufnehmen. Warte auf neues Foto...";

                string? found = null;
                var timeout = TimeSpan.FromSeconds(30);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.Elapsed < timeout)
                {
                    foreach (var d in candidateDirs)
                    {
                        if (!Directory.Exists(d)) continue;
                        foreach (var f in Directory.GetFiles(d))
                        {
                            if (existing.Contains(f)) continue;
                            try
                            {
                                var write = File.GetLastWriteTimeUtc(f);
                                if (write >= startUtc.AddSeconds(-2))
                                {
                                    found = f;
                                    break;
                                }
                            }
                            catch { }
                        }
                        if (found != null) break;
                    }
                    if (found != null) break;
                    System.Threading.Thread.Sleep(500);
                }

                if (found == null)
                {
                    lblStatus.Text = "Kein neues Foto automatisch gefunden — bitte Datei wählen.";
                    var ofd2 = new WpfOpenFileDialog
                    {
                        InitialDirectory = picRoot,
                        Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp|All files|*.*"
                    };
                    if (ofd2.ShowDialog() != true)
                    {
                        lblStatus.Text = string.Empty;
                        return;
                    }
                    chosenPath = ofd2.FileName;
                }
                else
                {
                    chosenPath = found;
                    lblStatus.Text = "Neues Foto automatisch gefunden.";
                }
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"Kamera konnte nicht gestartet werden: {ex.Message}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        else
        {
            var ofd = new WpfOpenFileDialog
            {
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp|All files|*.*"
            };
            if (ofd.ShowDialog() != true) return;
            chosenPath = ofd.FileName;
        }

        if (string.IsNullOrEmpty(chosenPath)) return;

        var destFileName = Path.GetFileName(chosenPath);
        var destPath = Path.Combine(rootDir, destFileName);
        try
        {
            File.Copy(chosenPath, destPath, true);
            selectedTask.PhotoPath = destPath;
            selectedTask.Done = true;

            lblStatus.Text = "Foto gespeichert.";
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show($"Fehler beim Speichern des Fotos: {ex.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
