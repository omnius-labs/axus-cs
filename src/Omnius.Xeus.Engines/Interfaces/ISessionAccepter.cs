using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Net;

namespace Omnius.Xeus.Engines
{
    public interface ISessionAccepter
    {
        ValueTask<ISession> AcceptAsync(string scheme, CancellationToken cancellationToken = default);

        ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
    }
}
