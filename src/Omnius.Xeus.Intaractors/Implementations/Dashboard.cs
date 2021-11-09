using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Intaractors.Internal;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Service.Remoting;

namespace Omnius.Xeus.Intaractors;

public sealed class Dashboard : AsyncDisposableBase, IDashboard
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly XeusServiceAdapter _service;
    private readonly IBytesPool _bytesPool;

    public static async ValueTask<Dashboard> CreateAsync(IXeusService xeusService, IBytesPool bytesPool, CancellationToken cancellationToken = default)
    {
        var dashboard = new Dashboard(xeusService, bytesPool);
        return dashboard;
    }

    private Dashboard(IXeusService xeusService, IBytesPool bytesPool)
    {
        _service = new XeusServiceAdapter(xeusService);
        _bytesPool = bytesPool;
    }

    protected override async ValueTask OnDisposeAsync()
    {
    }

    public async ValueTask<IEnumerable<SessionReport>> GetSessionsReportAsync(CancellationToken cancellationToken = default)
    {
        return await _service.GetSessionReportsAsync(cancellationToken);
    }

    public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
    {
        return await _service.GetMyNodeLocationAsync(cancellationToken);
    }

    public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        await _service.AddCloudNodeLocationsAsync(nodeLocations, cancellationToken);
    }
}