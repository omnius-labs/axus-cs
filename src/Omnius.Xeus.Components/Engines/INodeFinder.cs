using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Components.Connectors;
using Omnius.Xeus.Components.Models;

namespace Omnius.Xeus.Components.Engines
{
    public interface INodeFinderFactory
    {
        ValueTask<INodeFinder> CreateAsync(NodeFinderOptions options, IEnumerable<IConnector> connectors,
            IBytesPool bytesPool);
    }

    public delegate void FetchResourceTag(Action<ResourceTag> append);

    public interface INodeFinder : IAsyncDisposable
    {
        event FetchResourceTag? PushFetchResourceTag;
        event FetchResourceTag? WantFetchResourceTag;

        ValueTask<NodeProfile> GetMyNodeProfile(CancellationToken cancellationToken = default);
        ValueTask AddCloudNodeProfiles(IEnumerable<NodeProfile> nodeProfiles, CancellationToken cancellationToken = default);
        ValueTask<NodeProfile[]> FindNodeProfiles(ResourceTag tag, CancellationToken cancellationToken = default);
    }
}
