using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Primitives
{
    public interface IPrimitiveStorage
    {
        ulong TotalUsingBytes { get; }

        ValueTask CheckConsistency(Action<CheckConsistencyReport> callback, CancellationToken cancellationToken = default);

        bool Contains(OmniHash rootHash, OmniHash targetHash);
        uint GetLength(OmniHash rootHash, OmniHash targetHash);

        bool TryRead(OmniHash rootHash, OmniHash targetHash, [NotNullWhen(true)] out IMemoryOwner<byte>? memoryOwner);
        bool TryWrite(OmniHash rootHash, OmniHash targetHash, ReadOnlySpan<byte> value);
    }
}
