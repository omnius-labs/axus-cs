using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;
using System.Collections.Generic;
using System.Threading;

namespace Omnius.Xeus.Service
{
    public interface IExplorerFactory
    {
        ValueTask<IExplorer> CreateAsync(string configPath, ExplorerOptions explorerOptions, 
            IEnumerable<IConnector> connectors, IBytesPool bytesPool);
    }

    public interface IExplorer
    {
        IAsyncEnumerable<NodeProfile> FindNodeProfilesAsync(CancellationToken cancellationToken = default);
    }
}
