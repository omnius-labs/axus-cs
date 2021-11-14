using Omnius.Core.Pipelines;
using Omnius.Xeus.Engines.Primitives;

namespace Omnius.Xeus.Engines;

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
