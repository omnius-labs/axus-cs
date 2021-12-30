using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

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
        if (Program.IsDesignMode)
        {
            var designViewModel = new SettingsWindowDesignViewModel();
            this.DataContext = designViewModel;
            return;
        }

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

        var viewModel = serviceProvider.GetRequiredService<SettingsWindowViewModel>();
        this.DataContext = viewModel;
        this.SetWindowStatus(viewModel?.Status?.Window);
    }

    protected override async ValueTask OnActivatedAsync()
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (Program.IsDesignMode) return;

        if (this.DataContext is SettingsWindowViewModel viewModel)
        {
            viewModel.Status.Window = this.GetWindowStatus();
            await viewModel.DisposeAsync();
        }
    }
}
