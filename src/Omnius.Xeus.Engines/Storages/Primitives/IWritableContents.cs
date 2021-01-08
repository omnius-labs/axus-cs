using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Primitives
{
    public interface IWritableContents : IReadOnlyContents
    {
        ValueTask WriteBlockAsync(OmniHash contentHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default);
    }
}
