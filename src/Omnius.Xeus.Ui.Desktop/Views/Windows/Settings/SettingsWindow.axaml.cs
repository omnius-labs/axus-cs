using Avalonia.Markup.Xaml;
using Omnius.Xeus.Ui.Desktop.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Windows;

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
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (this.ViewModel is SettingsWindowViewModel viewModel)
        {
            await viewModel.DisposeAsync();
        }
    }

    public SettingsWindowViewModel? ViewModel
    {
        get => this.DataContext as SettingsWindowViewModel;
        set => this.DataContext = value;
    }
}
