using Omnius.Core.Pipelines;
using Omnius.Xeus.Service.Engines.Primitives;

namespace Omnius.Xeus.Service.Engines
{
    public interface INodeFinderEvents
    {
        IEventSubscriber<IContentExchanger> GetContentExchanger { get; }
    }
}
