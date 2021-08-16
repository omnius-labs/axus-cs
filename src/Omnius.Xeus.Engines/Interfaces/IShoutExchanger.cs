using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public interface IShoutExchanger : IContentExchanger, IAsyncDisposable
    {
        ValueTask<ShoutExchangerReport> GetReportAsync(CancellationToken cancellationToken = default);
    }
}
