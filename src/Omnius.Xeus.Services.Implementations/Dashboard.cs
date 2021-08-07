using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Daemon;
using Omnius.Xeus.Services.Models;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Services
{
    public sealed class Dashboard : AsyncDisposableBase, IDashboard
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        internal sealed class DashboardFactory : IDashboardFactory
        {
            public async ValueTask<IDashboard> CreateAsync(IXeusService xeusService, IBytesPool bytesPool, CancellationToken cancellationToken = default)
            {
                var result = new Dashboard(xeusService, bytesPool);
                await result.InitAsync(cancellationToken);

                return result;
            }
        }

        public static IDashboardFactory Factory { get; } = new DashboardFactory();

        public Dashboard(IXeusService xeusService, IBytesPool bytesPool)
        {
            _xeusService = xeusService;
            _bytesPool = bytesPool;
        }

        public async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
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

        public async ValueTask<EnginesModels.NodeProfile> GetMyNodeProfileAsync(CancellationToken cancellationToken = default)
        {
            var result = await _xeusService.GetMyNodeProfileAsync(cancellationToken);
            return result.NodeProfile;
        }

        public async ValueTask AddCloudNodeProfileAsync(IEnumerable<EnginesModels.NodeProfile> nodeProfiles, CancellationToken cancellationToken = default)
        {
            var request = new AddCloudNodeProfilesRequest(nodeProfiles.ToArray());
            await _xeusService.AddCloudNodeProfilesAsync(request, cancellationToken);
        }
    }
}
