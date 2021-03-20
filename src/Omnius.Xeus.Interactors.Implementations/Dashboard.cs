using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Interactors
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

        public async ValueTask<IEnumerable<ConnectionReport>> GetConnectionReportsAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<ConnectionReport>();

            var output = await _xeusService.CkadMediator_GetReportAsync(cancellationToken);
            results.AddRange(output.Report.Connections.Select(n => new ConnectionReport("ckad_mediator", n.HandshakeType, n.Address)));

            return results;
        }

        public async ValueTask<EnginesModels.NodeProfile> GetMyNodeProfileAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.CkadMediator_GetMyNodeProfileAsync(cancellationToken);
            return output.NodeProfile;
        }

        public async ValueTask AddCloudNodeProfileAsync(IEnumerable<EnginesModels.NodeProfile> nodeProfiles, CancellationToken cancellationToken = default)
        {
            var input = new CkadMediator_AddCloudNodeProfiles_Input(nodeProfiles.ToArray());
            await _xeusService.CkadMediator_AddCloudNodeProfilesAsync(input, cancellationToken);
        }
    }
}
