using Omnius.Axis.Models;

namespace Omnius.Axis.Engines;

public interface IFileExchanger : IAsyncDisposable
{
    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
}
