using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Components.Models;

namespace Omnius.Xeus.Components.Storages.Primitives
{
    public interface IWritableDeclaredMessageStorage : IReadOnlyDeclaredMessageStorage
    {
        ValueTask WriteMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
    }
}
