using Omnius.Axis.Engines.Primitives;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public sealed partial class NodeFinder
{
    private sealed class Events : INodeFinderEvents
    {
        public Events(IEventSubscriber<IContentExchanger> getContentExchanger)
        {
            this.GetContentExchangers = getContentExchanger;
        }

        public IEventSubscriber<IContentExchanger> GetContentExchangers { get; }
    }
}
