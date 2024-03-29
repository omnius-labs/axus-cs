using Omnius.Axus.Messages;

namespace Omnius.Axus.Engines.Primitives;

public interface IWritableShoutStorage : IReadOnlyShoutStorage
{
    ValueTask WriteShoutAsync(Shout shout, CancellationToken cancellationToken = default);
}
