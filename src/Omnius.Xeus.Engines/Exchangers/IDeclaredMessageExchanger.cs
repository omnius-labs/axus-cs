using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Engines.Engines
{
    public interface IDeclaredMessageExchangerFactory
    {
        ValueTask<IDeclaredMessageExchanger> CreateAsync(DeclaredMessageExchangerOptions options, IEnumerable<IConnector> connectors,
            ICkadMediator nodeFinder, IPushDeclaredMessageStorage pushStorage, IWantDeclaredMessageStorage wantStorage,
            IBytesPool bytesPool);
    }

    public interface IDeclaredMessageExchanger : IEngine, IAsyncDisposable
    {
    }
}
