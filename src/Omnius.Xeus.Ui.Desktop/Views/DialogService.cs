using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core.Avalonia;

namespace Omnius.Xeus.Ui.Desktop.Windows;

public interface IDialogService
{
    ValueTask<string> GetTextWindowAsync();

    ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync();
}

public class DialogService : IDialogService
{
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IMainWindowProvider _mainWindowProvider;

    public DialogService(IApplicationDispatcher applicationDispatcher, IMainWindowProvider mainWindowProvider)
    {
        _applicationDispatcher = applicationDispatcher;
        _mainWindowProvider = mainWindowProvider;
    }

    public async ValueTask<string> GetTextWindowAsync()
    {
        return await _applicationDispatcher.InvokeAsync(async () =>
        {
            var serviceProvider = await Bootstrapper.Instance.GetServiceProvider();
            if (serviceProvider is null) throw new NullReferenceException();

            var window = new TextWindow
            {
                ViewModel = serviceProvider.GetRequiredService<TextWindowViewModel>(),
            };

            await window.ShowDialog(_mainWindowProvider.GetMainWindow());
            return window.ViewModel.Text.Value;
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
