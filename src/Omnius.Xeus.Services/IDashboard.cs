using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Daemon;
using Omnius.Xeus.Services.Models;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Services
{
    public interface IDashboardFactory
    {
        ValueTask<IDashboard> CreateAsync(IXeusService xeusService, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IDashboard : IAsyncDisposable
    {
        ValueTask<IEnumerable<ConnectionsReport>> GetConnectionReportsAsync(CancellationToken cancellationToken = default);

        ValueTask<EnginesModels.NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default);

        ValueTask AddCloudNodeLocationAsync(IEnumerable<EnginesModels.NodeLocation> nodeLocations, CancellationToken cancellationToken = default);
    }
}
