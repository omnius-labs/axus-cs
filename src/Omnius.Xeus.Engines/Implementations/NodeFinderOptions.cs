using System.Collections.Generic;
using Omnius.Core;
using Omnius.Xeus.Engines.Primitives;

namespace Omnius.Xeus.Engines
{
    public record NodeFinderOptions
    {
        public string? ConfigDirectoryPath { get; init; }

        public uint MaxSessionCount { get; init; }

        public IReadOnlyCollection<ISessionConnector>? Connectors { get; init; }

        public IReadOnlyCollection<ISessionAccepter>? Accepters { get; init; }

        public IReadOnlyCollection<IContentExchanger>? Exchangers { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }
}
