using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core.Avalonia;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Ui.Desktop.Windows.Dialogs
{
    public interface IDialogService
    {
        ValueTask<IEnumerable<NodeLocation>> ShowAddNodesWindowAsync();
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

        public async ValueTask<IEnumerable<NodeLocation>> ShowAddNodesWindowAsync()
        {
            return await _applicationDispatcher.InvokeAsync(async () =>
            {
                var window = new AddNodesWindow
                {
                    ViewModel = Bootstrapper.Instance.ServiceProvider?.GetRequiredService<AddNodesWindowViewModel>() ?? throw new NullReferenceException(),
                };

                await window.ShowDialog(_mainWindowProvider.GetMainWindow());
                return window.ViewModel.GetNodeLocations();
            });
        }
    }
}
