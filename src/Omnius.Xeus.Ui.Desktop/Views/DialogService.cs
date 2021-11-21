using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core.Avalonia;

namespace Omnius.Xeus.Ui.Desktop.Windows;

public interface IDialogService
{
    ValueTask<string> ShowTextWindowAsync();

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

    public async ValueTask<string> ShowTextWindowAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var serviceProvider = await Bootstrapper.Instance.GetServiceProvider();
            if (serviceProvider is null) throw new NullReferenceException();

            var viewModel = serviceProvider.GetRequiredService<TextWindowViewModel>();
            await viewModel.InitializeAsync();

            var window = new TextWindow { ViewModel = viewModel, };
            await window.ShowDialog(_mainWindowProvider.GetMainWindow());

            return window.ViewModel.Text.Value;
        });
    }

    public async ValueTask ShowSettingsWindowAsync()
    {
        await _applicationDispatcher.InvokeAsync(async () =>
        {
            var serviceProvider = await Bootstrapper.Instance.GetServiceProvider();
            if (serviceProvider is null) throw new NullReferenceException();

            var viewModel = serviceProvider.GetRequiredService<SettingsWindowViewModel>();
            await viewModel.InitializeAsync();

            var window = new SettingsWindow { ViewModel = viewModel };
            await window.ShowDialog(_mainWindowProvider.GetMainWindow());
        });
    }

    public async ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var dialog = new OpenFileDialog();
            return await dialog.ShowAsync(_mainWindowProvider.GetMainWindow());
        });
    }
}
