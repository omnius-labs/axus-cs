using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Models;
using Omnius.Xeus.Remoting;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
{
    public sealed class Dashboard : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        public Dashboard(IXeusService xeusService, IBytesPool bytesPool)
        {
            _xeusService = xeusService;
            _bytesPool = bytesPool;
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask<IEnumerable<ConnectionsReport>> GetConnectionReportsAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<ConnectionsReport>();

            // var ckadMediator_output = await _xeusService.CkadMediator_GetReportAsync(cancellationToken);
            // results.Add(new ConnectionsReport("ckad_mediator", ckadMediator_output.Report.Connections.ToArray()));

            // var contentExchanger_output = await _xeusService.ContentExchanger_GetReportAsync(cancellationToken);
            // results.Add(new ConnectionsReport("content_exchanger", contentExchanger_output.Report.Connections.ToArray()));

            // var declaredMessageExchanger_output = await _xeusService.DeclaredMessageExchanger_GetReportAsync(cancellationToken);
            // results.Add(new ConnectionsReport("declared_message_exchanger", declaredMessageExchanger_output.Report.Connections.ToArray()));

            return results;
        }

        public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
        {
            var result = await _xeusService.GetMyNodeLocationAsync(cancellationToken);
            return result.NodeLocation;
        }

        public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
        {
            var request = new AddCloudNodeLocationsRequest(nodeLocations.ToArray());
            await _xeusService.AddCloudNodeLocationsAsync(request, cancellationToken);
        }
    }
}
