using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Views.Dialogs;
using Omnius.Axis.Ui.Desktop.Views.Settings;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Internal;

public interface IDialogService
{
    ValueTask<string> ShowMultiLineTextInputWindowAsync();

    ValueTask ShowSettingsWindowAsync();

    ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync();
}

public class DialogService : IDialogService
{
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IMainWindowProvider _mainWindowProvider;
    private readonly IClipboardService _clipboardService;

    public DialogService(IApplicationDispatcher applicationDispatcher, IMainWindowProvider mainWindowProvider, IClipboardService clipboardService)
    {
        _applicationDispatcher = applicationDispatcher;
        _mainWindowProvider = mainWindowProvider;
        _clipboardService = clipboardService;
    }

    public async ValueTask<string> ShowMultiLineTextInputWindowAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var window = new MultiLineTextInputWindow();
            var serviceProvider = Bootstrapper.Instance.GetServiceProvider();
            window.ViewModel = serviceProvider.GetRequiredService<MultiLineTextInputWindowViewModel>();

            await window.ShowDialog(_mainWindowProvider.GetMainWindow());
            return window.GetResult() ?? string.Empty;
        });
    }

    public async ValueTask ShowSettingsWindowAsync()
    {
        await _applicationDispatcher.InvokeAsync(async () =>
        {
            var window = new SettingsWindow();
            var serviceProvider = Bootstrapper.Instance.GetServiceProvider();
            window.ViewModel = serviceProvider.GetRequiredService<SettingsWindowViewModel>();

            await window.ShowDialog(_mainWindowProvider.GetMainWindow());
        });
    }

    public async ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var dialog = new OpenFileDialog();
            dialog.AllowMultiple = true;

            return await dialog.ShowAsync(_mainWindowProvider.GetMainWindow());
        });
    }
}
