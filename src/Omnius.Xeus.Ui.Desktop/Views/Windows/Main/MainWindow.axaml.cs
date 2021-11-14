using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Xeus.Ui.Desktop.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Windows;

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
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (this.ViewModel is MainWindowViewModel viewModel)
        {
            await viewModel.DisposeAsync();
        }

        await Bootstrapper.Instance.DisposeAsync();
    }

    public MainWindowViewModel? ViewModel
    {
        get => this.DataContext as MainWindowViewModel;
        set => this.DataContext = value;
    }
}
