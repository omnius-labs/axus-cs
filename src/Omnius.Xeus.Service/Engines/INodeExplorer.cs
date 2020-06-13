using System.Threading.Tasks;
using Omnius.Core;
using System.Collections.Generic;
using System.Threading;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Drivers;

namespace Omnius.Xeus.Service.Engines
{
    public interface INodeExplorerFactory
    {
        ValueTask<INodeExplorer> CreateAsync(string configPath, NodeExplorerOptions options,
            IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
            IPublishStorage publishStorage, IWantStorage wantStorage, IBytesPool bytesPool);
    }

    public interface INodeExplorer
    {
        ValueTask AddCloudNodeProfiles(IEnumerable<NodeProfile> nodeProfiles, CancellationToken cancellationToken = default);
        ValueTask<NodeProfile[]> FindNodeProfiles(OmniHash tag, CancellationToken cancellationToken = default);
        ValueTask<NodeProfile> GetMyNodeProfile(CancellationToken cancellationToken = default);
    }
}
