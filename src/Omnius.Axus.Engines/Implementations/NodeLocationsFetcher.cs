using Omnius.Axus.Models;

namespace Omnius.Axus.Engines;

public class NodeLocationsFetcher : INodeLocationsFetcher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private const string URL = "http://app.omnius-labs.com/axus/v1/nodes.txt";

    public NodeLocationsFetcher()
    {
    }

    public async ValueTask<IEnumerable<NodeLocation>> FetchAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);

        try
        {
            using var client = new HttpClient();
            var text = await client.GetStringAsync(URL, cancellationToken);

            var results = new List<NodeLocation>();

            foreach (var line in text.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!AxusUriConverter.TryDecode<NodeLocation>("node", 1, line, out var nodeLocation)) continue;
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
