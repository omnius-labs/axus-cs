using Omnius.Core.Net;
using Omnius.Core.Net.Connections;

namespace Omnius.Axus.Core.Engine.Models;

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
