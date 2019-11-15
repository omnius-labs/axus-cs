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
    class MessageStorage : IMessageStorage
    {
        public ulong TotalBytes { get; }

        public event EventHandler<IEnumerable<OmniHash>> BlocksAdded;
        public event EventHandler<IEnumerable<OmniHash>> BlocksRemoved;

        public ValueTask<IEnumerable<Clue>> ComputeRequiredBlocksForMessage(Clue clue)
        {
            throw new NotImplementedException();
        }

        public bool Contains(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OmniHash> ExceptFrom(IEnumerable<OmniHash> collection)
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

        public ValueTask<Clue> ImportMessage(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection)
        {
            throw new NotImplementedException();
        }

        public void RemoveMessage(Clue clue)
        {
            throw new NotImplementedException();
        }

        public bool TryRead(OmniHash hash, [NotNullWhen(true)] out IMemoryOwner<byte>? memoryOwner)
        {
            throw new NotImplementedException();
        }
    }
}
