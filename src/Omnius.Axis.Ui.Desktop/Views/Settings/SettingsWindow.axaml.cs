using System.ComponentModel;
using Avalonia.Markup.Xaml;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

public class SettingsWindow : StatefulWindowBase<SettingsWindowViewModelBase>
{
    public SettingsWindow()
        : base()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnInitialized()
    {
        if (this.ViewModel?.Status is SettingsWindowStatus status)
        {
            this.SetWindowStatus(status.Window);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (this.ViewModel?.Status is SettingsWindowStatus status)
        {
            status.Window = this.GetWindowStatus();
        }
    }
}
