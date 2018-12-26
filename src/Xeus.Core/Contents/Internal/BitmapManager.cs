using System;
using System.Collections.Generic;
using System.IO;
using Omnix.Base;
using Omnix.Io;
using Xeus.Core.Internal;

namespace Xeus.Core.Contents.Internal
{
    internal sealed class BitmapManager : DisposableBase
    {
        private BufferPool _bufferPool;

        private byte[][] _bufferList;
        private ulong _length;

        private volatile bool _disposed;

        public static readonly uint SectorSize = 32 * 1024;

        public BitmapManager(BufferPool bufferPool)
        {
            _bufferPool = bufferPool;
        }

        public ulong Length => _length;

        public void SetLength(ulong length)
        {
            ulong byteCount = MathHelper.Roundup(length, 8);
            ulong sectorCount = MathHelper.Roundup(byteCount, SectorSize);

            _bufferList = new byte[sectorCount][];

            for (int i = 0; i < _bufferList.Length; i++)
            {
                var buffer = _bufferPool.GetArrayPool().Rent((int)SectorSize);
                BytesOperations.Zero(buffer);

                _bufferList[i] = buffer;
            }

            _length = length;
        }

        public bool Get(ulong point)
        {
            if (point >= _length) throw new ArgumentOutOfRangeException(nameof(point));

            ulong sectorOffset = (point / 8) / SectorSize;
            int bufferOffset = (int)((point / 8) % SectorSize);
            byte bitOffset = (byte)(point % 8);

            var buffer = _bufferList[sectorOffset];
            return ((buffer[bufferOffset] << bitOffset) & 0x80) == 0x80;
        }

        public void Set(ulong point, bool state)
        {
            if (point >= _length) throw new ArgumentOutOfRangeException(nameof(point));

            ulong sectorOffset = (point / 8) / SectorSize;
            int bufferOffset = (int)((point / 8) % SectorSize);
            byte bitOffset = (byte)(point % 8);

            var buffer = _bufferList[sectorOffset];

            if (state)
            {
                buffer[bufferOffset] |= (byte)(0x80 >> bitOffset);
            }
            else
            {
                buffer[bufferOffset] &= (byte)(~(0x80 >> bitOffset));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                for (int i = 0; i < _bufferList.Length; i++)
                {
                    _bufferPool.GetArrayPool().Return(_bufferList[i]);
                }

                _bufferList.Clone();
            }
        }
    }
}
