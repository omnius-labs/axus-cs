using Omnius.Axis.Models;

namespace Omnius.Axis.Engines.Primitives;

public interface IContentExchanger
{
    IEnumerable<ContentClue> GetPushContentClues();

    IEnumerable<ContentClue> GetWantContentClues();
}
