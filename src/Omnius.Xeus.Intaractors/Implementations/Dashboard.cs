using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Intaractors
{
    public sealed class Dashboard : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IXeusApi _xeusApi;
        private readonly IBytesPool _bytesPool;

        public Dashboard(IXeusApi xeusApi, IBytesPool bytesPool)
        {
            _xeusApi = xeusApi;
            _bytesPool = bytesPool;
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask<IEnumerable<SessionReport>> GetSessionsReportAsync(CancellationToken cancellationToken = default)
        {
            return await _xeusApi.GetSessionsReportAsync(cancellationToken);
        }

        public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
        {
            return await _xeusApi.GetMyNodeLocationAsync(cancellationToken);
        }

        public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
        {
            await _xeusApi.AddCloudNodeLocationsAsync(nodeLocations, cancellationToken);
        }
    }
}
