using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axus.Ui.Desktop.Configuration;
using Omnius.Axus.Ui.Desktop.Windows.Dialogs.MultilineTextEdit;
using Omnius.Axus.Ui.Desktop.Windows.Dialogs.SinglelineTextEdit;
using Omnius.Axus.Ui.Desktop.Windows.Settings;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Internal;

public interface IDialogService
{
    ValueTask<string> ShowMultilineTextEditAsync();
    ValueTask<string> ShowSinglelineTextEditAsync();
    ValueTask ShowSettingsWindowAsync();
    ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync();
    ValueTask<string?> ShowOpenDirectoryWindowAsync();
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

    public async ValueTask<string> ShowMultilineTextEditAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var window = new MultilineTextEditWindow(Path.Combine(_axusEnvironment.DatabaseDirectoryPath, "windows", "multiline_text_edit"));
            var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

            var viewModel = serviceProvider.GetRequiredService<MultilineTextEditWindowModel>();
            window.ViewModel = viewModel;

            await window.ShowDialog(_mainWindowProvider.GetMainWindow());
            return window.GetResult() ?? string.Empty;
        });
    }

    public async ValueTask<string> ShowSinglelineTextEditAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var window = new SinglelineTextEditWindow(Path.Combine(_axusEnvironment.DatabaseDirectoryPath, "windows", "singleline_text_edit"));
            var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

            var viewModel = serviceProvider.GetRequiredService<SinglelineTextEditWindowModel>();
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

    public async ValueTask<string?> ShowOpenDirectoryWindowAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var dialog = new OpenFolderDialog();

            return await dialog.ShowAsync(_mainWindowProvider.GetMainWindow());
        });
    }
}
