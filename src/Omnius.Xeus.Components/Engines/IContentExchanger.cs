using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Components.Connectors;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Components.Storages;

namespace Omnius.Xeus.Components.Engines
{
    public interface IContentExchangerFactory
    {
        ValueTask<IContentExchanger> CreateAsync(ContentExchangerOptions options, IEnumerable<IConnector> connectors,
            INodeFinder nodeFinder, IPushContentStorage pushStorage, IWantContentStorage wantStorage, IBytesPool bytesPool);
    }

    public interface IContentExchanger : IEngine, IAsyncDisposable
    {
    }
}
