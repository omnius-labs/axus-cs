using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines.Primitives;

public interface IWritableShoutStorage : IReadOnlyShoutStorage
{
    ValueTask WriteShoutAsync(Shout shout, CancellationToken cancellationToken = default);
}