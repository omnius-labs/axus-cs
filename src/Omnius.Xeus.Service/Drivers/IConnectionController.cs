using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using System.Collections.Generic;
using Omnius.Core;
using Omnius.Core.Network.Connections;

namespace Omnius.Xeus.Service.Drivers
{
    public readonly struct ConnectionControllerAcceptResult
    {
        public ConnectionControllerAcceptResult(IConnection connection, OmniAddress address)
        {
            this.Connection = connection;
            this.Address = address;
        }

        public IConnection Connection { get; }
        public OmniAddress Address { get; }
    }

    public interface IConnectionControllerFactory
    {
        public ValueTask<IConnectionController> CreateAsync(ConnectionControllerOptions options, IBytesPool bytesPool);
    }

    public interface IConnectionController : IAsyncDisposable
    {
        ValueTask<IConnection?> ConnectAsync(OmniAddress address, string serviceType, CancellationToken cancellationToken = default);
        ValueTask<ConnectionControllerAcceptResult> AcceptAsync(string serviceType, CancellationToken cancellationToken = default);
        ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
    }
}
