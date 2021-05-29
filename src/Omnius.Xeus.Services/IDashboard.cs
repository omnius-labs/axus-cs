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
        ValueTask<IEnumerable<GroupedConnectionReport>> GetConnectionReportsAsync(CancellationToken cancellationToken = default);

        ValueTask<EnginesModels.NodeProfile> GetMyNodeProfileAsync(CancellationToken cancellationToken = default);

        ValueTask AddCloudNodeProfileAsync(IEnumerable<EnginesModels.NodeProfile> nodeProfiles, CancellationToken cancellationToken = default);
    }
}
