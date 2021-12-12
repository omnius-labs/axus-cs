using Omnius.Axis.Intaractors.Internal;
using Omnius.Axis.Models;
using Omnius.Axis.Remoting;
using Omnius.Core;

namespace Omnius.Axis.Intaractors;

public sealed class Dashboard : AsyncDisposableBase, IDashboard
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly AxisServiceAdapter _service;
    private readonly IBytesPool _bytesPool;

    public static async ValueTask<Dashboard> CreateAsync(IAxisService axisService, IBytesPool bytesPool, CancellationToken cancellationToken = default)
    {
        var dashboard = new Dashboard(axisService, bytesPool);
        return dashboard;
    }

    private Dashboard(IAxisService axisService, IBytesPool bytesPool)
    {
        _service = new AxisServiceAdapter(axisService);
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
