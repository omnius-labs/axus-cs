using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;

namespace Xeus.Engine
{
    public interface IStorageAggregator : IMessageStorage, IFileStorage
    {
    }

    public interface IPrimitiveStorage : IDisposable
    {
        ulong TotalUsingBytes { get; }

        ValueTask CheckConsistency(Action<CheckConsistencyStatus> callback, CancellationToken cancellationToken = default);

        bool Contains(OmniHash hash);
        uint GetLength(OmniHash hash);

        bool TryRead(OmniHash hash, [NotNullWhen(true)] out IMemoryOwner<byte>? memoryOwner);
        bool TryWrite(OmniHash hash, ReadOnlySpan<byte> value);
    }

    public interface IMessageStorage : IPrimitiveStorage
    {
        ValueTask<Clue> AddPublishMessage(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);
        void RemovePublishMessage(Clue clue);
        IEnumerable<PublishMessageStatus> GetPublishMessageStatuses();
        ValueTask<bool> TryExportPublishMessage(Clue clue, IBufferWriter<byte> bufferWriter);

        void AddWantMessage(Clue clue);
        void RemoveWantMessage(Clue clue);
        IEnumerable<WantMessageStatus> GetWantMessageStatuses();
    }

    public interface IFileStorage : IPrimitiveStorage
    {
        ValueTask<Clue> AddPublishFile(string path, CancellationToken cancellationToken = default);
        void RemovePublishFile(string path);
        IEnumerable<PublishFileStatus> GetPublishFileStatuses();

        void AddWantFile(Clue clue, string path);
        void RemoveWantFile(Clue clue, string path);
        IEnumerable<WantFileStatus> GetWantFileStatuses();
    }
}
