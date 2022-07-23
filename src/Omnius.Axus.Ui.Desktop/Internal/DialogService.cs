using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axus.Ui.Desktop.Models;
using Omnius.Axus.Ui.Desktop.Windows.Settings;
using Omnius.Axus.Ui.Desktop.Windows.TextEdit;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Internal;

public interface IDialogService
{
    ValueTask<string> ShowTextEditWindowAsync();

    ValueTask ShowSettingsWindowAsync();

    ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync();
}

public class DialogService : IDialogService
{
    private readonly AxusEnvironment _axusEnvironment;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IMainWindowProvider _mainWindowProvider;
    private readonly IClipboardService _clipboardService;

    public DialogService(AxusEnvironment axusEnvironment, IApplicationDispatcher applicationDispatcher, IMainWindowProvider mainWindowProvider, IClipboardService clipboardService)
    {
        _axusEnvironment = axusEnvironment;
        _applicationDispatcher = applicationDispatcher;
        _mainWindowProvider = mainWindowProvider;
        _clipboardService = clipboardService;
    }

    public async ValueTask<string> ShowTextEditWindowAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var window = new TextEditWindow(Path.Combine(_axusEnvironment.DatabaseDirectoryPath, "windows", "text_edit"));
            var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

            var viewModel = serviceProvider.GetRequiredService<TextEditWindowModel>();
            window.ViewModel = viewModel;

            await window.ShowDialog(_mainWindowProvider.GetMainWindow());
            return window.GetResult() ?? string.Empty;
        });
    }

    public async ValueTask ShowSettingsWindowAsync()
    {
        await _applicationDispatcher.InvokeAsync(async () =>
        {
            var window = new SettingsWindow(Path.Combine(_axusEnvironment.DatabaseDirectoryPath, "windows", "settings"));
            var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

            var viewModel = serviceProvider.GetRequiredService<SettingsWindowModel>();
            await viewModel.InitializeAsync();
            window.ViewModel = viewModel;

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
        }) ?? Enumerable.Empty<string>();
    }
}
