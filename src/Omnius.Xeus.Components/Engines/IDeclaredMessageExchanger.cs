using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Components.Connectors;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Components.Storages;

namespace Omnius.Xeus.Components.Engines
{
    public interface IDeclaredMessageExchangerFactory
    {
        ValueTask<IDeclaredMessageExchanger> CreateAsync(DeclaredMessageExchangerOptions options, IEnumerable<IConnector> connectors,
            INodeFinder nodeFinder, IPushDeclaredMessageStorage pushStorage, IWantDeclaredMessageStorage wantStorage,
            IBytesPool bytesPool);
    }

    public interface IDeclaredMessageExchanger : IEngine, IAsyncDisposable
    {
    }
}
