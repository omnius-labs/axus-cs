using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Components.Primitives
{
    public interface IPrimitiveStorage : IDisposable
    {
        ulong TotalUsingBytes { get; }

        ValueTask CheckConsistency(Action<CheckConsistencyReport> callback, CancellationToken cancellationToken = default);

        bool Contains(OmniHash hash);
        uint GetLength(OmniHash hash);

        bool TryRead(OmniHash hash, [NotNullWhen(true)] out IMemoryOwner<byte>? memoryOwner);
        bool TryWrite(OmniHash hash, ReadOnlySpan<byte> value);
    }
}
