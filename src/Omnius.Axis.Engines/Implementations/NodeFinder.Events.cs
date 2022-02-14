using Omnius.Axis.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public sealed partial class NodeFinder
{
    private sealed class Events : INodeFinderEvents
    {
        public Events(IEventListener<IEnumerable<ContentClue>> getPushContentClues, IEventListener<IEnumerable<ContentClue>> getWantContentClues)
        {
            this.GetPushContentClues = getPushContentClues;
            this.GetWantContentClues = getWantContentClues;
        }

        public IEventListener<IEnumerable<ContentClue>> GetPushContentClues { get; }

        public IEventListener<IEnumerable<ContentClue>> GetWantContentClues { get; }
    }
}
