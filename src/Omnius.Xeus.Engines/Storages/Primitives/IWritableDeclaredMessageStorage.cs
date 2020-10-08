using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Engines.Storages.Primitives
{
    public interface IWritableDeclaredMessageStorage : IReadOnlyDeclaredMessageStorage
    {
        ValueTask WriteMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
    }
}
