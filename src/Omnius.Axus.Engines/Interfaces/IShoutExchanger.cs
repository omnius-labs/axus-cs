using Omnius.Axus.Messages;

namespace Omnius.Axus.Engines;

public interface IShoutExchanger : IAsyncDisposable
{
    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
}
