using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Net;

namespace Omnius.Xeus.Service.Engines;

public interface ISessionConnector : IAsyncDisposable
{
    ValueTask<ISession?> ConnectAsync(OmniAddress address, string scheme, CancellationToken cancellationToken = default);
}