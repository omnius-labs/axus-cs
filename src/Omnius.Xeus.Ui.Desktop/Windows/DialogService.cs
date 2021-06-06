using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core.Avalonia;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Ui.Desktop.Windows.AddNodes;

namespace Omnius.Xeus.Ui.Desktop.Windows
{
    public interface IDialogService
    {
        ValueTask<IEnumerable<NodeProfile>> OpenAddNodesWindowAsync();
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

        public async ValueTask<IEnumerable<NodeProfile>> OpenAddNodesWindowAsync()
        {
            var window = await _applicationDispatcher.InvokeAsync(() => new AddNodesWindow());
            window.ViewModel = Bootstrapper.ServiceProvider?.GetRequiredService<AddNodesWindowViewModel>() ?? throw new NullReferenceException();

            await window.ShowDialog(_mainWindowProvider.GetMainWindow());
            return window.ViewModel.GetNodeProfiles();
        }
    }
}
