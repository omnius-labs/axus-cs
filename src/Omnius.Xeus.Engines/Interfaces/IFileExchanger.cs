using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public interface IFileExchanger : IContentExchanger, IAsyncDisposable
    {
        ValueTask<FileExchangerReport> GetReportAsync(CancellationToken cancellationToken = default);
    }
}
