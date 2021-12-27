using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Windows;

public class SettingsWindow : StatefulWindowBase
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

    protected override async ValueTask OnInitializeAsync()
    {
        var serviceProvider = await Bootstrapper.Instance.GetServiceProvider();
        if (serviceProvider is null) throw new NullReferenceException();

        this.ViewModel = serviceProvider.GetRequiredService<SettingsWindowViewModel>();
        this.SetWindowStatus(this.ViewModel?.Status?.Window);
    }

    protected override async ValueTask OnActivatedAsync()
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (this.ViewModel is not null)
        {
            this.ViewModel.Status.Window = this.GetWindowStatus();
            await this.ViewModel.DisposeAsync();
        }
    }

    public SettingsWindowViewModel? ViewModel
    {
        get => this.DataContext as SettingsWindowViewModel;
        set => this.DataContext = value;
    }
}
