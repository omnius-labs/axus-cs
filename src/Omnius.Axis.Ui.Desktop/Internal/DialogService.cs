using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Models;
using Omnius.Axis.Ui.Desktop.TextEdit;
using Omnius.Axis.Ui.Desktop.Windows.Settings;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Internal;

public interface IDialogService
{
    ValueTask<string> ShowTextEditWindowAsync();

    ValueTask ShowSettingsWindowAsync();

    ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync();
}

public class DialogService : IDialogService
{
    private readonly AxisEnvironment _axisEnvironment;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IMainWindowProvider _mainWindowProvider;
    private readonly IClipboardService _clipboardService;

    public DialogService(AxisEnvironment axisEnvironment, IApplicationDispatcher applicationDispatcher, IMainWindowProvider mainWindowProvider, IClipboardService clipboardService)
    {
        _axisEnvironment = axisEnvironment;
        _applicationDispatcher = applicationDispatcher;
        _mainWindowProvider = mainWindowProvider;
        _clipboardService = clipboardService;
    }

    public async ValueTask<string> ShowTextEditWindowAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var window = new TextEditWindow(Path.Combine(_axisEnvironment.DatabaseDirectoryPath, "windows", "multi_line_text_input"));
            var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

            var viewModel = serviceProvider.GetRequiredService<TextEditWindowModel>();
            await viewModel.InitializeAsync();
            window.ViewModel = viewModel;

            await window.ShowDialog(_mainWindowProvider.GetMainWindow());
            return window.GetResult() ?? string.Empty;
        });
    }

    public async ValueTask ShowSettingsWindowAsync()
    {
        await _applicationDispatcher.InvokeAsync(async () =>
        {
            var window = new SettingsWindow(Path.Combine(_axisEnvironment.DatabaseDirectoryPath, "windows", "settings"));
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
