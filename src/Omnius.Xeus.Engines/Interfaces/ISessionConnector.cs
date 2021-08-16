using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Net;

namespace Omnius.Xeus.Engines
{
    public interface ISessionConnector
    {
        ValueTask<ISession?> ConnectAsync(OmniAddress address, string scheme, CancellationToken cancellationToken = default);
    }
}
