using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Cryptography;

namespace Xeus.Core.Storages
{
    class StorageAggregator : IStorageAggregator
    {
        public ulong TotalProtectedBytes { get; }
        public ulong TotalUsingBytes { get; }
        public ulong TotalFreeBytes { get; }
        public ulong TotalBytes { get; }

        public event EventHandler<IEnumerable<OmniHash>> BlocksAdded;
        public event EventHandler<IEnumerable<OmniHash>> BlocksRemoved;

        public ValueTask CheckBlocks(Action<CheckBlocksProgressStatus> callback, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IEnumerable<Clue>> ComputeRequiredBlocksForFile(Clue clue)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IEnumerable<Clue>> ComputeRequiredBlocksForMessage(Clue clue)
        {
            throw new NotImplementedException();
        }

        public bool Contains(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public void Delete(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OmniHash> ExceptFrom(IEnumerable<OmniHash> collection)
        {
            throw new NotImplementedException();
        }

        public ValueTask ExportFile(Clue clue, string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ExportMessage(Clue clue, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OmniHash> GetHashes()
        {
            throw new NotImplementedException();
        }

        public uint GetLength(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Clue> ImportFile(string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Clue> ImportMessage(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection)
        {
            throw new NotImplementedException();
        }

        public void Lock(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public void RemoveFile(string path)
        {
            throw new NotImplementedException();
        }

        public void RemoveMessage(Clue clue)
        {
            throw new NotImplementedException();
        }

        public void Resize(ulong size)
        {
            throw new NotImplementedException();
        }

        public bool TryRead(OmniHash hash, [NotNullWhen(true)] out IMemoryOwner<byte>? memoryOwner)
        {
            throw new NotImplementedException();
        }

        public bool TryWrite(OmniHash hash, ReadOnlySpan<byte> value)
        {
            throw new NotImplementedException();
        }

        public void Unlock(OmniHash hash)
        {
            throw new NotImplementedException();
        }
    }
}
