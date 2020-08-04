using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Connectors;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Service.Storages;

namespace Omnius.Xeus.Service.Engines
{
    public interface IDeclaredMessageExchangerFactory
    {
        ValueTask<IDeclaredMessageExchanger> CreateAsync(DeclaredMessageExchangerOptions options, IEnumerable<IConnector> connectors,
            INodeFinder nodeFinder, IPushDeclaredMessageStorage pushStorage, IWantDeclaredMessageStorage wantStorage,
            IBytesPool bytesPool);
    }

    public interface IDeclaredMessageExchanger : IAsyncDisposable
    {
    }
}
