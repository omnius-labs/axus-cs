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
        var serviceProvider = await Bootstrapper.Instance.GetServiceProvider();
        if (serviceProvider is null) throw new NullReferenceException();

        this.ViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();
        this.SetWindowStatus(this.ViewModel.Status?.Window);
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

        await Bootstrapper.Instance.DisposeAsync();
    }

    public MainWindowViewModel? ViewModel
    {
        get => this.DataContext as MainWindowViewModel;
        set => this.DataContext = value;
    }
}
