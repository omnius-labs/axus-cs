using Omnius.Axis.Intaractors;
using Omnius.Axis.Models;

namespace Omnius.Axis.Ui.Desktop.Internal;

public interface INodesFetcher
{
    ValueTask<IEnumerable<NodeLocation>> FetchAsync(CancellationToken cancellationToken = default);
}

public class NodesFetcher : INodesFetcher
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private const string URL = "http://app.omnius-labs.com/axis/v1/nodes.txt";

    public async ValueTask<IEnumerable<NodeLocation>> FetchAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            var text = await client.GetStringAsync(URL, cancellationToken);

            var results = new List<NodeLocation>();

            foreach (var line in text.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!AxisMessage.TryStringToNode(line, out var nodeLocation)) continue;
                results.Add(nodeLocation);
            }

            return results.ToArray();
        }
        catch (Exception e)
        {
            _logger.Warn(e);
        }

        return Enumerable.Empty<NodeLocation>();
    }
}
