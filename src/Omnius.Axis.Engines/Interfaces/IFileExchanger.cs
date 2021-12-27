using Omnius.Axis.Engines.Primitives;
using Omnius.Axis.Models;

namespace Omnius.Axis.Engines;

public interface IFileExchanger : IContentExchanger, IAsyncDisposable
{
    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
}
