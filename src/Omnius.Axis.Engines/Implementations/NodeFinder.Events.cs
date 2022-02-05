using Omnius.Axis.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public sealed partial class NodeFinder
{
    private sealed class Events : INodeFinderEvents
    {
        public Events(IEventSubscriber<IEnumerable<ContentClue>> getPushContentClues, IEventSubscriber<IEnumerable<ContentClue>> getWantContentClues)
        {
            this.GetPushContentClues = getPushContentClues;
            this.GetWantContentClues = getWantContentClues;
        }

        public IEventSubscriber<IEnumerable<ContentClue>> GetPushContentClues { get; }

        public IEventSubscriber<IEnumerable<ContentClue>> GetWantContentClues { get; }
    }
}
