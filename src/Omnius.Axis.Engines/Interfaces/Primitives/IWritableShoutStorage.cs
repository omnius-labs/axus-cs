using Omnius.Axis.Models;

namespace Omnius.Axis.Engines.Primitives;

public interface IWritableShoutStorage : IReadOnlyShoutStorage
{
    ValueTask WriteShoutAsync(Shout shout, CancellationToken cancellationToken = default);
}
