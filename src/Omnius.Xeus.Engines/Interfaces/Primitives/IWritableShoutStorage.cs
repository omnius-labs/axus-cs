using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines.Primitives;

public interface IWritableShoutStorage : IReadOnlyShoutStorage
{
    ValueTask WriteShoutAsync(Shout shout, CancellationToken cancellationToken = default);
}
