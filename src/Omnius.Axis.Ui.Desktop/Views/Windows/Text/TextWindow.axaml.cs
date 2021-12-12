using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Windows;

public class TextWindow : StatefulWindowBase
{
    public TextWindow()
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

        this.ViewModel = serviceProvider.GetRequiredService<TextWindowViewModel>();
        this.SetWindowStatus(this.ViewModel?.Status?.Window);
    }

    protected override async ValueTask OnActivatedAsync()
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (this.ViewModel is TextWindowViewModel viewModel)
        {
            this.ViewModel.Status.Window = this.GetWindowStatus();
            await viewModel.DisposeAsync();
        }
    }

    public TextWindowViewModel? ViewModel
    {
        get => this.DataContext as TextWindowViewModel;
        set => this.DataContext = value;
    }
}
