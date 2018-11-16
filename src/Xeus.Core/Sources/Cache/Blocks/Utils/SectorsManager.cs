using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Omnius.Base;
using Omnius.Io;

namespace Amoeba.Service
{
    partial class CacheManager
    {
        public partial class BlocksManager
        {
            sealed class SectorsManager : DisposableBase
            {
                private BitmapManager _bitmapManager;
                private BufferManager _bufferManager;

                private HashSet<long> _freeSectors = new HashSet<long>();
                private long _size;

                private long _totalSectorCount;
                private long _usingSectorCount;

                private volatile bool _disposed;

                public static readonly int SectorSize = 1024 * 256;

                public SectorsManager(BufferManager bufferManager)
                {
                    _bitmapManager = new BitmapManager(bufferManager);
                    _bufferManager = bufferManager;
                }

                private static long Roundup(long value, long unit)
                {
                    if (value % unit == 0) return value;
                    else return ((value / unit) + 1) * unit;
                }

                public long Size
                {
                    get
                    {
                        return _size;
                    }
                }

                public long UsingSectorCount
                {
                    get
                    {
                        return _usingSectorCount;
                    }
                }

                public long FreeSectorCount
                {
                    get
                    {
                        return _totalSectorCount - _usingSectorCount;
                    }
                }

                public void Reallocate(long size)
                {
                    _freeSectors.Clear();
                    _size = size;

                    _totalSectorCount = Roundup(_size, SectorSize) / SectorSize;
                    _usingSectorCount = 0;

                    _bitmapManager.SetLength(_totalSectorCount);
                }

                public void SetUsingSectors(IEnumerable<long> sectors)
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

                public void SetFreeSectors(IEnumerable<long> sectors)
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

                public IEnumerable<long> TakeFreeSectors(int count)
                {
                    if (_freeSectors.Count < count)
                    {
                        for (long i = 0; i < _bitmapManager.Length; i++)
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
    }
}
