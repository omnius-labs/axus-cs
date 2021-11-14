using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines.Primitives;

public interface IContentExchanger
{
    IEnumerable<ContentClue> GetPushContentClues();

    IEnumerable<ContentClue> GetWantContentClues();
}
