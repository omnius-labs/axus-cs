using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;
using System.Collections.Generic;
using System.Threading;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service
{
    public interface INodeExplorerFactory
    {
        ValueTask<INodeExplorer> CreateAsync(string configPath, ExplorerOptions explorerOptions, IObjectStoreFactory objectStoreFactory,
            IEnumerable<IConnector> connectors, IEnumerable<IPublishStorage> publishStorage, IEnumerable<IWantStorage> wantStorages,
            IBytesPool bytesPool);
    }

    public interface INodeExplorer
    {
        IAsyncEnumerable<NodeProfile> FindNodeProfilesAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
    }
}
