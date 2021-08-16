using System.Collections.Generic;
using Omnius.Core;

namespace Omnius.Xeus.Engines.Exchangers
{
    public record FileExchangerOptions
    {
        public IReadOnlyCollection<ISessionConnector>? Connectors { get; init; }

        public IReadOnlyCollection<ISessionAccepter>? Accepters { get; init; }

        public INodeFinder? NodeFinder { get; init; }

        public IPublishedFileStorage? PublishedFileStorage { get; init; }

        public ISubscribedFileStorage? SubscribedFileStorage { get; init; }

        public IBytesPool? BytesPool { get; init; }

        public uint MaxSessionCount { get; init; }
    }
}
