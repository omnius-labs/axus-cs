using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core.Avalonia;

namespace Omnius.Xeus.Ui.Desktop.Windows
{
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
                var window = new TextWindow
                {
                    ViewModel = Bootstrapper.Instance.ServiceProvider?.GetRequiredService<TextWindowViewModel>() ?? throw new NullReferenceException(),
                };

                await window.ShowDialog(_mainWindowProvider.GetMainWindow());
                return window.ViewModel.Text.Value;
            });
        }

        public async ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync()
        {
            var dialog = new OpenFileDialog();
            return await dialog.ShowAsync(_mainWindowProvider.GetMainWindow());
        }
    }
}
