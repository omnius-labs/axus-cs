using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;

namespace Omnius.Xeus.Engines
{
    public interface IConnectionConnector
    {
        ValueTask<IConnection?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default);
    }
}
