using Omnius.Axis.Engines.Primitives;
using Omnius.Axis.Models;

namespace Omnius.Axis.Engines;

public interface IShoutExchanger : IContentExchanger, IAsyncDisposable
{
    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
}
