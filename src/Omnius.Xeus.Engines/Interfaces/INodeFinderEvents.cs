using Omnius.Core.Pipelines;
using Omnius.Xeus.Engines.Primitives;

namespace Omnius.Xeus.Engines;

public interface INodeFinderEvents
{
    IEventSubscriber<IContentExchanger> GetContentExchangers { get; }
}
