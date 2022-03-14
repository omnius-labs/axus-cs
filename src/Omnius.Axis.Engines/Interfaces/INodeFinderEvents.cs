using Omnius.Axis.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public interface INodeFinderEvents
{
    IFuncListener<IEnumerable<ContentClue>> GetPushContentClues { get; }

    IFuncListener<IEnumerable<ContentClue>> GetWantContentClues { get; }
}
