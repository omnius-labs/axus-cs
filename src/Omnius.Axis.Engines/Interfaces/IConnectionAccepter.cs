using Omnius.Core.Net;
using Omnius.Core.Net.Connections;

namespace Omnius.Axis.Engines;

public record ConnectionAcceptedResult
{
    public ConnectionAcceptedResult(IConnection connection, OmniAddress address)
    {
        this.Connection = connection;
        this.Address = address;
    }

    public IConnection Connection { get; }

    public OmniAddress Address { get; }
}

public interface IConnectionAccepter : IAsyncDisposable
{
    ValueTask<ConnectionAcceptedResult?> AcceptAsync(CancellationToken cancellationToken = default);

    ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
}
