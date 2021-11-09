using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Service.Engines.Primitives;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines;

public interface IShoutExchanger : IContentExchanger, IAsyncDisposable
{
    ValueTask<ShoutExchangerReport> GetReportAsync(CancellationToken cancellationToken = default);
}