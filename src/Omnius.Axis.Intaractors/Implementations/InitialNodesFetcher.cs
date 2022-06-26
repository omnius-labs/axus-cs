using Omnius.Axis.Models;
using Omnius.Core;

namespace Omnius.Axis.Intaractors;

public class InitialNodesFetcher : AsyncDisposableBase, IInitialNodesFetcher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxisServiceMediator _axisServiceMediator;

    private Task _fetchTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private const string URL = "http://app.omnius-labs.com/axis/v1/nodes.txt";

    public static async ValueTask<InitialNodesFetcher> CreateAsync(IAxisServiceMediator axisServiceMediator, CancellationToken cancellationToken = default)
    {
        var initialNodesFetcher = new InitialNodesFetcher(axisServiceMediator);
        await initialNodesFetcher.InitAsync(cancellationToken);
        return initialNodesFetcher;
    }

    private InitialNodesFetcher(IAxisServiceMediator axisServiceMediator)
    {
        _axisServiceMediator = axisServiceMediator;
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        _fetchTask = this.FetchAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _fetchTask;
        _cancellationTokenSource.Dispose();
    }

    private async Task FetchAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            var nodeLocations = await this.InternalFetchAsync(cancellationToken);
            await _axisServiceMediator.AddCloudNodeLocationsAsync(nodeLocations, cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask<IEnumerable<NodeLocation>> InternalFetchAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            var text = await client.GetStringAsync(URL, cancellationToken);

            var results = new List<NodeLocation>();

            foreach (var line in text.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!AxisMessageConverter.TryStringToNode(line, out var nodeLocation)) continue;
                results.Add(nodeLocation);
            }

            return results.ToArray();
        }
        catch (Exception e)
        {
            _logger.Warn(e, "Failed to fetch node locations");
        }

        return Enumerable.Empty<NodeLocation>();
    }

    public new ValueTask DisposeAsync() => throw new NotImplementedException();
}
