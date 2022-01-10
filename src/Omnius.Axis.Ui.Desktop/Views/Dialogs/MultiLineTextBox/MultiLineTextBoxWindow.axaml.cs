using System.ComponentModel;
using Avalonia.Markup.Xaml;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Views.Dialogs;

public partial class MultiLineTextBoxWindow : StatefulWindowBase<MultiLineTextBoxWindowViewModel>
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

    protected override void OnInitialized()
    {
        if (this.ViewModel?.Status is MultiLineTextBoxWindowStatus status)
        {
            this.SetWindowStatus(status.Window);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (this.ViewModel?.Status is MultiLineTextBoxWindowStatus status)
        {
            status.Window = this.GetWindowStatus();
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
