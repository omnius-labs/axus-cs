using Omnius.Axus.Models;

namespace Omnius.Axus.Engines.Primitives;

public interface IWritableShoutStorage : IReadOnlyShoutStorage
{
    ValueTask WriteShoutAsync(Shout shout, CancellationToken cancellationToken = default);
}
