using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IKadexExplorerFactory
    {
        ValueTask<IKadexExplorer> CreateAsync(string configPath, IEnumerable<IConnector> connectors, IBufferPool<byte> bufferPool);
    }

    public interface IKadexExplorer : IExplorer
    {
        public static IKadexExplorerFactory Factory { get; }
    }
}
