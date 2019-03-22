using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Omnix.Base;
using Omnix.Io;
using Xeus.Core.Internal;

namespace Xeus.Core.Contents.Internal
{
    internal sealed class UsingSectorsManager : DisposableBase
    {
        private BitmapManager _bitmapManager;
        private BufferPool _bufferPool;

        private HashSet<ulong> _freeSectors = new HashSet<ulong>();
        private ulong _size;

        private ulong _totalSectorCount;
        private ulong _usingSectorCount;

        private volatile bool _disposed;

        public static readonly uint SectorSize = 1024 * 256;

        public UsingSectorsManager(BufferPool bufferPool)
        {
            _bitmapManager = new BitmapManager(bufferPool);
            _bufferPool = bufferPool;
        }

        public ulong Size => _size;

        public ulong UsingSectorCount => _usingSectorCount;

        public ulong FreeSectorCount => _totalSectorCount - _usingSectorCount;

        public void Reallocate(ulong size)
        {
            _freeSectors.Clear();
            _size = size;

            _totalSectorCount = MathHelper.Roundup(_size, SectorSize) / SectorSize;
            _usingSectorCount = 0;

        }

        public void SetUsingSectors(IEnumerable<ulong> sectors)
        {
            foreach (var sector in sectors)
            {
                if (!_bitmapManager.Get(sector))
                {
                    _bitmapManager.Set(sector, true);

                    _usingSectorCount++;
                }
            }

            _freeSectors.ExceptWith(sectors);
        }

        public void SetFreeSectors(IEnumerable<ulong> sectors)
        {
            foreach (var sector in sectors)
            {
                if (_bitmapManager.Get(sector))
                {
                    _bitmapManager.Set(sector, false);
                    if (_freeSectors.Count < 1024 * 4) _freeSectors.Add(sector);

                    _usingSectorCount--;
                }
            }
        }

        public ulong[] TakeFreeSectors(int count)
        {
            if (_freeSectors.Count < count)
            {
                for (ulong i = 0; i < _bitmapManager.Length; i++)
                {
                    if (!_bitmapManager.Get(i))
                    {
                        _freeSectors.Add(i);
                        if (_freeSectors.Count >= Math.Max(count, 1024 * 4)) break;
                    }
                }
            }

            var result = _freeSectors.Take(count).ToArray();

            foreach (var sector in result)
            {
                _bitmapManager.Set(sector, true);

                _usingSectorCount++;
            }

            _freeSectors.ExceptWith(result);

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                if (_bitmapManager != null)
                {
                    try
                    {
                        _bitmapManager.Dispose();
                    }
                    catch (Exception)
                    {

                    }

                    _bitmapManager = null;
                }
            }
        }
    }
}
