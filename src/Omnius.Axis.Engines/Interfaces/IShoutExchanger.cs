using Omnius.Axis.Models;

namespace Omnius.Axis.Engines;

public interface IShoutExchanger : IAsyncDisposable
{
    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
}
