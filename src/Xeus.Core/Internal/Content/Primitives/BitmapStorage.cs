using System;
using Omnix.Base;
using Xeus.Core.Internal.Helpers;

namespace Xeus.Core.Internal.Content.Primitives
{
    internal sealed class BitmapStorage : DisposableBase
    {
        private readonly BufferPool _bufferPool;

        private byte[][]? _bufferList;
        private ulong _length;

        public static readonly uint SectorSize = 32 * 1024; // 32 KB

        public BitmapStorage(BufferPool bufferPool)
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
            if (point >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(point));
            }

            if (_bufferList is null)
            {
                return false;
            }

            ulong sectorOffset = (point / 8) / SectorSize;
            int bufferOffset = (int)((point / 8) % SectorSize);
            byte bitOffset = (byte)(point % 8);

            var buffer = _bufferList[sectorOffset];
            return ((buffer[bufferOffset] << bitOffset) & 0x80) == 0x80;
        }

        public void Set(ulong point, bool state)
        {
            if (point >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(point));
            }

            if (_bufferList is null)
            {
                return;
            }

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
            if (disposing)
            {
                if (_bufferList != null)
                {
                    for (int i = 0; i < _bufferList.Length; i++)
                    {
                        _bufferPool.GetArrayPool().Return(_bufferList[i]);
                    }

                    _bufferList = null;
                }
            }
        }
    }
}
