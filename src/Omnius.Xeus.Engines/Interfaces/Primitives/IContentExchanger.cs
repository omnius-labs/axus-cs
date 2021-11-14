using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines.Primitives;

public interface IContentExchanger
{
    IEnumerable<ContentClue> GetPushContentClues();

    IEnumerable<ContentClue> GetWantContentClues();
}
