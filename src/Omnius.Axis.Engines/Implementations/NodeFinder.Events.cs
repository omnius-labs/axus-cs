using Omnius.Axis.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public sealed partial class NodeFinder
{
    private sealed class Events : INodeFinderEvents
    {
        public Events(IFuncListener<IEnumerable<ContentClue>> getPushContentClues, IFuncListener<IEnumerable<ContentClue>> getWantContentClues)
        {
            this.GetPushContentClues = getPushContentClues;
            this.GetWantContentClues = getWantContentClues;
        }

        public IFuncListener<IEnumerable<ContentClue>> GetPushContentClues { get; }

        public IFuncListener<IEnumerable<ContentClue>> GetWantContentClues { get; }
    }
}
