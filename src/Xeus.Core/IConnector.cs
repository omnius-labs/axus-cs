using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Network;

namespace Xeus.Core
{
    public interface IConnector : IService
    {
        ValueTask<ConnectorResult> AcceptAsync(CancellationToken token = default);
        ValueTask<ConnectorResult> ConnectAsync(OmniAddress address, CancellationToken token = default);
        void SetOptions(ConnectorOptions options);
    }
}
