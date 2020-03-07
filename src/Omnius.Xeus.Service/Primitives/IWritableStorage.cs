using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Primitives
{
    public interface IWritableStorage : IReadOnlyStorage
    {
        ValueTask WriteAsync(OmniHash rootHash, OmniHash targetHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default);
    }
}
