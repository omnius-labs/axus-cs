using Omnius.Axis.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public interface INodeFinderEvents
{
    IEventSubscriber<IEnumerable<ContentClue>> GetPushContentClues { get; }

    IEventSubscriber<IEnumerable<ContentClue>> GetWantContentClues { get; }
}
