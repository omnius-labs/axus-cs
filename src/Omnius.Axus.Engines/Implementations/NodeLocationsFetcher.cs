using Omnius.Axus.Messages;

namespace Omnius.Axus.Engines;

public class NodeLocationsFetcher : INodeLocationsFetcher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly string? _uri;
    private readonly NodeLocationsFetcherOperationType _operationType;

    public static NodeLocationsFetcher Create(NodeLocationsFetcherOptions options)
    {
        var nodeLocationsFetcher = new NodeLocationsFetcher(options);
        return nodeLocationsFetcher;
    }

    private NodeLocationsFetcher(NodeLocationsFetcherOptions options)
    {
        _uri = options.Uri;
        _operationType = options.OperationType;
    }

    public async ValueTask<IEnumerable<NodeLocation>> FetchAsync(CancellationToken cancellationToken = default)
    {
        if (_operationType == NodeLocationsFetcherOperationType.HttpGet && _uri is not null)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            try
            {
                using var client = new HttpClient();
                var text = await client.GetStringAsync(_uri, cancellationToken);

                var results = new List<NodeLocation>();

                foreach (var line in text.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!AxusUriConverter.Instance.TryStringToNodeLocation(line, out var nodeLocation)) continue;
                    results.Add(nodeLocation);
                }

                return results.ToArray();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Failed to fetch node locations");
            }
        }

        return Enumerable.Empty<NodeLocation>();
    }
}
