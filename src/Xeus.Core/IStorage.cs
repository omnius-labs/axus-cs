using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Cryptography;

namespace Xeus.Core
{
    public interface IStorage : IService, IEnumerable<OmniHash>
    {
        event Action<IEnumerable<OmniHash>> AddedBlockEvents;

        event Action<IEnumerable<OmniHash>> RemovedBlockEvents;

        event Action<IEnumerable<ErrorReport>> ErrorReportEvents;

        ulong ProtectionBytes { get; }

        ulong UsingBytes { get; }

        ValueTask CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token);

        IEnumerable<OmniHash> ExceptFrom(IEnumerable<OmniHash> collection);

        IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection);

        uint GetLength(OmniHash hash);

        bool Contains(OmniHash hash);

        OmniHash[] ToArray();

        void Lock(OmniHash hash);

        void Unlock(OmniHash hash);

        ulong Size { get; }

        void Resize(ulong size);

        bool TryGet(OmniHash hash, out IMemoryOwner<byte>? memoryOwner);

        bool TrySet(OmniHash hash, ReadOnlySpan<byte> value);

        void Remove(OmniHash hash);
    }
}
