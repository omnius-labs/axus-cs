using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Engines.Exchangers
{
    public interface IDeclaredMessageExchangerFactory
    {
        ValueTask<IDeclaredMessageExchanger> CreateAsync(DeclaredMessageExchangerOptions options, IEnumerable<IConnector> connectors,
            ICkadMediator nodeFinder, IDeclaredMessagePublisher pushStorage, IDeclaredMessageSubscriber wantStorage, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IDeclaredMessageExchanger : IAsyncDisposable
    {
        ValueTask<DeclaredMessageExchangerReport> GetReportAsync(CancellationToken cancellationToken = default);
    }
}
