using Omnius.Axus.Models;

namespace Omnius.Axus.Engines;

public interface IFileExchanger : IAsyncDisposable
{
    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
}
