using Omnius.Axus.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Engines;

public sealed partial class NodeFinder
{
    private sealed class Events : INodeFinderEvents
    {
        public Events(IFuncListener<IEnumerable<ContentClue>> getPushContentClues, IFuncListener<IEnumerable<ContentClue>> getWantContentClues)
        {
            this.GetPushContentCluesListener = getPushContentClues;
            this.GetWantContentCluesListener = getWantContentClues;
        }

        public IFuncListener<IEnumerable<ContentClue>> GetPushContentCluesListener { get; }

        public IFuncListener<IEnumerable<ContentClue>> GetWantContentCluesListener { get; }
    }
}
