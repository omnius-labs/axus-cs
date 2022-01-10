using System.ComponentModel;
using Avalonia.Markup.Xaml;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public partial class MainWindow : StatefulWindowBase<MainWindowViewModelBase>
{
    public MainWindow()
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
        if (this.ViewModel?.Status is MainWindowStatus status)
        {
            this.SetWindowStatus(status.Window);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (this.ViewModel?.Status is MainWindowStatus status)
        {
            status.Window = this.GetWindowStatus();
        }
    }
}
