using Omnius.Axis.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public interface INodeFinderEvents
{
    IEventListener<IEnumerable<ContentClue>> GetPushContentClues { get; }

    IEventListener<IEnumerable<ContentClue>> GetWantContentClues { get; }
}
