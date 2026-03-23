using System.Windows;
using System.Windows.Controls;
using WpfButton = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfPanel = System.Windows.Controls.Panel;
using WpfSize = System.Windows.Size;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace mitoSoft.Checklist.Helpers;

public class UiModeManager(Window window)
{
    private readonly Window _window = window;
    private readonly Dictionary<string, ButtonConfig> _buttonConfigs = InitializeButtonConfigs();
    private readonly PanelConfig _panelConfig = new();
    private readonly LabelConfig _labelConfig = new();
    private bool _isTabletMode;

    private static Dictionary<string, ButtonConfig> InitializeButtonConfigs()
    {
        return new Dictionary<string, ButtonConfig>
        {
            ["toggleMode"] = new ButtonConfig
            {
                DesktopStyle = "MainButtonStyle",
                TabletStyle = "MainButtonStyleTablet",
                DesktopSize = new WpfSize(60, 40),
                TabletSize = new WpfSize(80, 60),
                DesktopMargin = new Thickness(0, 0, 10, 0),
                TabletMargin = new Thickness(0, 0, 15, 0),
                DesktopFontSize = 14,
                TabletFontSize = 24
            },
            ["btnLoad"] = new ButtonConfig
            {
                DesktopStyle = "MainButtonStyle",
                TabletStyle = "MainButtonStyleTablet",
                DesktopSize = new WpfSize(60, 40),
                TabletSize = new WpfSize(80, 60),
                DesktopMargin = new Thickness(0, 0, 10, 0),
                TabletMargin = new Thickness(0, 0, 15, 0)
            },
            ["btnLoadTemplate"] = new ButtonConfig
            {
                DesktopStyle = "MainButtonStyle",
                TabletStyle = "MainButtonStyleTablet",
                DesktopSize = new WpfSize(60, 40),
                TabletSize = new WpfSize(80, 60),
                DesktopMargin = new Thickness(0, 0, 10, 0),
                TabletMargin = new Thickness(0, 0, 15, 0)
            },
            ["btnSave"] = new ButtonConfig
            {
                DesktopStyle = "MainButtonStyle",
                TabletStyle = "MainButtonStyleTablet",
                DesktopSize = new WpfSize(60, 40),
                TabletSize = new WpfSize(80, 60),
                DesktopMargin = new Thickness(0, 0, 10, 0),
                TabletMargin = new Thickness(0, 0, 15, 0)
            },
            ["btnSaveAs"] = new ButtonConfig
            {
                DesktopStyle = "MainButtonStyle",
                TabletStyle = "MainButtonStyleTablet",
                DesktopSize = new WpfSize(60, 40),
                TabletSize = new WpfSize(80, 60),
                DesktopMargin = new Thickness(0, 0, 10, 0),
                TabletMargin = new Thickness(0, 0, 15, 0)
            },
            ["btnAttachPhoto"] = new ButtonConfig
            {
                DesktopStyle = "MainButtonStyle",
                TabletStyle = "MainButtonStyleTablet",
                DesktopSize = new WpfSize(60, 40),
                TabletSize = new WpfSize(80, 60),
                DesktopMargin = new Thickness(0, 0, 10, 0),
                TabletMargin = new Thickness(0, 0, 15, 0)
            },
            ["btnExportPdf"] = new ButtonConfig
            {
                DesktopStyle = "MainButtonStyle",
                TabletStyle = "MainButtonStyleTablet",
                DesktopSize = new WpfSize(60, 40),
                TabletSize = new WpfSize(80, 60),
                DesktopMargin = new Thickness(0, 0, 0, 0),
                TabletMargin = new Thickness(0, 0, 0, 0)
            },
            ["btnBack"] = new ButtonConfig
            {
                DesktopStyle = "PrimaryButtonStyle",
                TabletStyle = "PrimaryButtonStyleTablet",
                DesktopSize = new WpfSize(200, 40),
                TabletSize = new WpfSize(250, 60),
                DesktopMargin = new Thickness(0),
                TabletMargin = new Thickness(0)
            },
            ["btnNext"] = new ButtonConfig
            {
                DesktopStyle = "PrimaryButtonStyle",
                TabletStyle = "PrimaryButtonStyleTablet",
                DesktopSize = new WpfSize(200, 40),
                TabletSize = new WpfSize(250, 60),
                DesktopMargin = new Thickness(0),
                TabletMargin = new Thickness(0)
            }
        };
    }

    public void ApplyMode(bool isTabletMode)
    {
        _isTabletMode = isTabletMode;
        ApplyButtonStyles();
        ApplyPanelStyles();
        ApplyLabelStyles();
        ApplyToggleButtonContent();
    }

    public void ToggleMode()
    {
        ApplyMode(!_isTabletMode);
    }

    private void ApplyButtonStyles()
    {
        foreach (var kvp in _buttonConfigs)
        {
            if (_window.FindName(kvp.Key) is WpfButton button)
            {
                var config = kvp.Value;
                button.Style = (Style)_window.FindResource(_isTabletMode ? config.TabletStyle : config.DesktopStyle);
                button.Width = _isTabletMode ? config.TabletSize.Width : config.DesktopSize.Width;
                button.Height = _isTabletMode ? config.TabletSize.Height : config.DesktopSize.Height;
                button.Margin = _isTabletMode ? config.TabletMargin : config.DesktopMargin;

                if (config.DesktopFontSize.HasValue && config.TabletFontSize.HasValue)
                {
                    button.FontSize = _isTabletMode ? config.TabletFontSize.Value : config.DesktopFontSize.Value;
                }
            }
        }
    }

    private void ApplyPanelStyles()
    {
        if (_window.FindName("buttonPanel") is WpfPanel panel)
        {
            panel.Height = _isTabletMode ? _panelConfig.TabletHeight : _panelConfig.DesktopHeight;
        }
    }

    private void ApplyLabelStyles()
    {
        // Font sizes remain the same for now, but can be adjusted if needed
        if (_window.FindName("lblPlanTitle") is WpfTextBlock lblPlanTitle)
        {
            lblPlanTitle.FontSize = _labelConfig.PlanTitleFontSize;
        }

        if (_window.FindName("lblStepDescription") is WpfTextBlock lblStepDescription)
        {
            lblStepDescription.FontSize = _labelConfig.StepDescriptionFontSize;
        }

        if (_window.FindName("lblStepIndex") is WpfTextBlock lblStepIndex)
        {
            lblStepIndex.FontSize = _labelConfig.StepIndexFontSize;
        }
    }

    private void ApplyToggleButtonContent()
    {
        if (_window.FindName("toggleMode") is WpfButton toggleButton)
        {
            toggleButton.Content = _isTabletMode ? "📱" : "💻";
        }
    }

    public WpfCheckBox CreateTaskCheckbox(int index, string text, bool isChecked, RoutedEventHandler checkedHandler, RoutedEventHandler uncheckedHandler)
    {
        var checkbox = new WpfCheckBox
        {
            Content = text,
            IsChecked = isChecked,
            Tag = index,
            FontSize = _isTabletMode ? 18 : 16,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            MinWidth = 30,
            MinHeight = 30
        };
        checkbox.Checked += checkedHandler;
        checkbox.Unchecked += uncheckedHandler;
        return checkbox;
    }

    public WpfTextBlock CreatePhotoLinkLabel(int taskIndex, string photoPath, System.Windows.Input.MouseButtonEventHandler clickHandler)
    {
        var link = new WpfTextBlock
        {
            Text = "(Foto)",
            Foreground = System.Windows.Media.Brushes.Blue,
            TextDecorations = TextDecorations.Underline,
            Cursor = System.Windows.Input.Cursors.Hand,
            Tag = taskIndex,
            FontSize = _isTabletMode ? 20 : 14,
            VerticalAlignment = VerticalAlignment.Center,
            ToolTip = photoPath
        };
        link.MouseLeftButtonUp += clickHandler;
        return link;
    }

    public System.Windows.Controls.TextBox CreateTextInputBox(int index, string? text, TextChangedEventHandler textChangedHandler)
    {
        var textBox = new System.Windows.Controls.TextBox
        {
            Text = text ?? string.Empty,
            Tag = index,
            FontSize = _isTabletMode ? 16 : 14,
            MinHeight = 30,
            Padding = _isTabletMode ? new Thickness(10) : new Thickness(5),
            TextWrapping = TextWrapping.NoWrap
        };

        textBox.TextChanged += textChangedHandler;
        return textBox;
    }

    public StackPanel CreateTextInputTask(Models.MaintenanceTask task, int index, TextChangedEventHandler textChangedHandler)
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

        var textBox = CreateTextInputBox(index, task.UserInput, textChangedHandler);
        panel.Children.Add(textBox);

        return panel;
    }

    public StackPanel CreateNumberInputTask(Models.MaintenanceTask task, int index, TextChangedEventHandler textChangedHandler, System.Windows.Input.TextCompositionEventHandler previewTextInputHandler)
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

        var numberBox = CreateTextInputBox(index, task.UserInput, textChangedHandler);
        numberBox.InputScope = new System.Windows.Input.InputScope
        {
            Names = { new System.Windows.Input.InputScopeName(System.Windows.Input.InputScopeNameValue.Number) }
        };
        numberBox.PreviewTextInput += previewTextInputHandler;

        panel.Children.Add(numberBox);

        return panel;
    }

    public StackPanel CreateCheckboxTask(Models.MaintenanceTask task, int index, RoutedEventHandler checkedHandler, RoutedEventHandler uncheckedHandler)
    {
        var panel = new StackPanel
        {
            Orientation = WpfOrientation.Horizontal,
            Margin = _isTabletMode ? new Thickness(0, 10, 0, 10) : new Thickness(0, 5, 0, 5)
        };

        var checkbox = CreateTaskCheckbox(index, task.Text, task.Done, checkedHandler, uncheckedHandler);
        panel.Children.Add(checkbox);

        return panel;
    }

    public StackPanel CreatePhotoTask(Models.MaintenanceTask task, int index, RoutedEventHandler checkedHandler, RoutedEventHandler uncheckedHandler, System.Windows.Input.MouseButtonEventHandler linkClickHandler)
    {
        var panel = new StackPanel
        {
            Orientation = WpfOrientation.Horizontal,
            Margin = _isTabletMode ? new Thickness(0, 10, 0, 10) : new Thickness(0, 5, 0, 5)
        };

        var checkbox = CreateTaskCheckbox(index, task.Text, task.Done, checkedHandler, uncheckedHandler);
        panel.Children.Add(checkbox);

        if (!string.IsNullOrEmpty(task.PhotoPath))
        {
            var link = CreatePhotoLinkLabel(index, task.PhotoPath, linkClickHandler);
            panel.Children.Add(link);
        }

        return panel;
    }

    private class ButtonConfig
    {
        public string DesktopStyle { get; init; } = string.Empty;
        public string TabletStyle { get; init; } = string.Empty;
        public WpfSize DesktopSize { get; init; }
        public WpfSize TabletSize { get; init; }
        public Thickness DesktopMargin { get; init; }
        public Thickness TabletMargin { get; init; }
        public double? DesktopFontSize { get; init; }
        public double? TabletFontSize { get; init; }
    }

    private class PanelConfig
    {
        public double DesktopHeight { get; init; } = 40;
        public double TabletHeight { get; init; } = 60;
    }

    private class LabelConfig
    {
        public double PlanTitleFontSize { get; init; } = 32;
        public double StepDescriptionFontSize { get; init; } = 14;
        public double StepIndexFontSize { get; init; } = 16;
    }
}
