using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Cryptography;

namespace Xeus.Core
{
    public interface IStorageAggregator : IMessageStorage, IFileStorage, ICacheStorage
    {
        ulong TotalProtectedBytes { get; }

        ValueTask CheckBlocks(Action<CheckBlocksProgressStatus> callback, CancellationToken token);

        void Lock(OmniHash hash);
        void Unlock(OmniHash hash);
    }

    public interface IPrimitiveStorage
    {
        event EventHandler<IEnumerable<OmniHash>> BlocksAdded;
        event EventHandler<IEnumerable<OmniHash>> BlocksRemoved;

        ulong TotalBytes { get; }

        bool Contains(OmniHash hash);
        IEnumerable<OmniHash> ExceptFrom(IEnumerable<OmniHash> collection);
        IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection);

        uint GetLength(OmniHash hash);

        IEnumerable<OmniHash> GetHashes();

        bool TryRead(OmniHash hash, [NotNullWhen(true)] out IMemoryOwner<byte>? memoryOwner);
    }

    public interface IMessageStorage : IPrimitiveStorage
    {
        ValueTask<Clue> ImportMessage(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);
        ValueTask<IEnumerable<Clue>> ComputeRequiredBlocksForMessage(Clue clue);
        ValueTask ExportMessage(Clue clue, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
        void RemoveMessage(Clue clue);
    }

    public interface IFileStorage : IPrimitiveStorage
    {
        ValueTask<Clue> ImportFile(string path, CancellationToken cancellationToken = default);
        ValueTask<IEnumerable<Clue>> ComputeRequiredBlocksForFile(Clue clue);
        ValueTask ExportFile(Clue clue, string path, CancellationToken cancellationToken = default);
        void RemoveFile(string path);
    }

    public interface ICacheStorage : IPrimitiveStorage
    {
        ulong TotalUsingBytes { get; }
        ulong TotalFreeBytes { get; }

        void Resize(ulong size);

        bool TryWrite(OmniHash hash, ReadOnlySpan<byte> value);
        void Delete(OmniHash hash);
    }
}
