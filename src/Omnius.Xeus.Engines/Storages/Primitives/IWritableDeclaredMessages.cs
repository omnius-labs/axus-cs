using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Engines.Storages.Primitives
{
    public interface IWritableDeclaredMessages : IReadOnlyDeclaredMessages
    {
        ValueTask WriteMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
    }
}
