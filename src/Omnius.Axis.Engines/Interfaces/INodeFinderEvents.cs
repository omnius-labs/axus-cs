using Omnius.Axis.Engines.Primitives;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public interface INodeFinderEvents
{
    IEventSubscriber<IContentExchanger> GetContentExchangers { get; }
}
