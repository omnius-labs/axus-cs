using Omnius.Core.Pipelines;
using Omnius.Xeus.Service.Engines.Primitives;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class NodeFinder
    {
        private sealed class NodeFinderEvents : INodeFinderEvents
        {
            public NodeFinderEvents(IEventSubscriber<IContentExchanger> getContentExchanger)
            {
                this.GetContentExchangers = getContentExchanger;
            }

            public IEventSubscriber<IContentExchanger> GetContentExchangers { get; }
        }
    }
}
