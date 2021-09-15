using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core.Avalonia;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Ui.Desktop.Windows
{
    public interface IDialogService
    {
        ValueTask<IEnumerable<NodeLocation>> ShowNodesWindowAsync();
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

        public async ValueTask<IEnumerable<NodeLocation>> ShowNodesWindowAsync()
        {
            return await _applicationDispatcher.InvokeAsync(async () =>
            {
                var window = new NodesWindow
                {
                    ViewModel = Bootstrapper.Instance.ServiceProvider?.GetRequiredService<NodesWindowViewModel>() ?? throw new NullReferenceException(),
                };

                await window.ShowDialog(_mainWindowProvider.GetMainWindow());
                return window.ViewModel.GetNodeLocations();
            });
        }

        public async ValueTask<IEnumerable<string>> ShowOpenFileWindowAsync()
        {
            var dialog = new OpenFileDialog();
            return await dialog.ShowAsync(_mainWindowProvider.GetMainWindow());
        }
    }
}
