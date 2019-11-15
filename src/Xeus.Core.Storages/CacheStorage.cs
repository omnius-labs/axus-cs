using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Omnix.Cryptography;

namespace Xeus.Core.Storages
{
    public class CacheStorage : ICacheStorage
    {
        public ulong TotalUsingBytes { get; }
        public ulong TotalFreeBytes { get; }
        public ulong TotalBytes { get; }

        public event EventHandler<IEnumerable<OmniHash>> BlocksAdded;
        public event EventHandler<IEnumerable<OmniHash>> BlocksRemoved;

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

        public IEnumerable<OmniHash> GetHashes()
        {
            throw new NotImplementedException();
        }

        public uint GetLength(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OmniHash> IntersectFrom(IEnumerable<OmniHash> collection)
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
    }
}
