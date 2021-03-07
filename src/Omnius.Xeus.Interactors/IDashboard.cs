using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Interactors
{
    public interface IDashboardFactory
    {
        ValueTask<IDashboard> CreateAsync(DashboardOptions options, CancellationToken cancellationToken = default);
    }

    public class DashboardOptions
    {
        public DashboardOptions(IXeusService xeusService, IBytesPool bytesPool)
        {
            this.XeusService = xeusService;
            this.BytesPool = bytesPool;
        }

        public IXeusService XeusService { get; }

        public IBytesPool BytesPool { get; }
    }

    public interface IDashboard : IAsyncDisposable
    {
        ValueTask<IEnumerable<ConnectionReport>> GetConnectionReports(CancellationToken cancellationToken = default);

        ValueTask<EnginesModels.NodeProfile> GetMyNodeProfile(CancellationToken cancellationToken = default);

        ValueTask AddCloudNodeProfile(IEnumerable<EnginesModels.NodeProfile> nodeProfiles, CancellationToken cancellationToken = default);
    }
}
