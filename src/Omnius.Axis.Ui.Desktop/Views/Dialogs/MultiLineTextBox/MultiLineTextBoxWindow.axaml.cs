using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Views.Dialogs;

public class MultiLineTextBoxWindow : StatefulWindowBase
{
    public MultiLineTextBoxWindow()
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
        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

        var viewModel = serviceProvider.GetRequiredService<MultiLineTextBoxWindowViewModel>();
        this.DataContext = viewModel;
        this.SetWindowStatus(viewModel?.Status?.Window);
    }

    protected override async ValueTask OnActivatedAsync()
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (this.DataContext is MultiLineTextBoxWindowViewModel viewModel)
        {
            viewModel.Status.Window = this.GetWindowStatus();
            await viewModel.DisposeAsync();
        }
    }

    public string? GetResult()
    {
        if (this.DataContext is MultiLineTextBoxWindowViewModel viewModel)
        {
            return viewModel.GetResult();
        }

        return null;
    }
}
