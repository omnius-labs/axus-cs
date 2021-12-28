using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Windows;

public partial class MainWindow : StatefulWindowBase
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

    protected override async ValueTask OnInitializeAsync()
    {
        if (Program.IsDesignMode) return;

        var serviceProvider = await Bootstrapper.Instance.GetServiceProvider();
        if (serviceProvider is null) throw new NullReferenceException();

        var viewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();
        this.SetWindowStatus(viewModel.Status?.Window);
    }

    protected override async ValueTask OnActivatedAsync()
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (Program.IsDesignMode) return;

        if (this.DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Status.Window = this.GetWindowStatus();
            await viewModel.DisposeAsync();
        }
    }
}
