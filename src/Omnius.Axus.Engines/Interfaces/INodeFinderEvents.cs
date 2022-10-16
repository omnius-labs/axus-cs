using Omnius.Axus.Models;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Engines;

public interface INodeFinderEvents
{
    IFuncListener<IEnumerable<ContentClue>> GetPushContentCluesListener { get; }
    IFuncListener<IEnumerable<ContentClue>> GetWantContentCluesListener { get; }
}
