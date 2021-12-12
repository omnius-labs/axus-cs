using Omnius.Axis.Engines.Primitives;
using Omnius.Axis.Models;

namespace Omnius.Axis.Engines;

public interface IFileExchanger : IContentExchanger, IAsyncDisposable
{
    ValueTask<FileExchangerReport> GetReportAsync(CancellationToken cancellationToken = default);
}
