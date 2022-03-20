using Omnius.Axis.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axis.Engines;

public interface INodeFinderEvents
{
    IFuncListener<IEnumerable<ContentClue>> GetPushContentCluesListener { get; }

    IFuncListener<IEnumerable<ContentClue>> GetWantContentCluesListener { get; }
}
