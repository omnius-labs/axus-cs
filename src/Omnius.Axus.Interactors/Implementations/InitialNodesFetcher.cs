using Omnius.Axus.Models;
using Omnius.Core;

namespace Omnius.Axus.Interactors;

public class InitialNodesFetcher : AsyncDisposableBase, IInitialNodesFetcher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IServiceMediator _axusServiceMediator;

    private Task _fetchTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private const string URL = "http://app.omnius-labs.com/axus/v1/nodes.txt";

    public static async ValueTask<InitialNodesFetcher> CreateAsync(IServiceMediator axusServiceMediator, CancellationToken cancellationToken = default)
    {
        var initialNodesFetcher = new InitialNodesFetcher(axusServiceMediator);
        await initialNodesFetcher.InitAsync(cancellationToken);
        return initialNodesFetcher;
    }

    private InitialNodesFetcher(IServiceMediator axusServiceMediator)
    {
        _axusServiceMediator = axusServiceMediator;
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
            await _axusServiceMediator.AddCloudNodeLocationsAsync(nodeLocations, cancellationToken);
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
                if (!AxusMessageConverter.TryStringToNode(line, out var nodeLocation)) continue;
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
}
