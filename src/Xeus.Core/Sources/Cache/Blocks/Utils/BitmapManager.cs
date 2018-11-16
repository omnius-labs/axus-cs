using System;
using System.IO;
using Omnius.Base;
using Omnius.Io;

namespace Amoeba.Service
{
    partial class CacheManager
    {
        public partial class BlocksManager
        {
            sealed class BitmapManager : DisposableBase
            {
                private Stream _bitmapStream;
                private BufferManager _bufferManager;

                private long _length;

                private bool _cacheChanged = false;
                private long _cacheSector = -1;

                private byte[] _cacheBuffer;
                private int _cacheBufferLength = 0;

                private volatile bool _disposed;

                public static readonly int SectorSize = 256;

                public BitmapManager(BufferManager bufferManager)
                {
                    _bitmapStream = new RecyclableMemoryStream(bufferManager);
                    _bufferManager = bufferManager;

                    _cacheBuffer = _bufferManager.TakeBuffer(SectorSize);
                }

                private static long Roundup(long value, long unit)
                {
                    if (value % unit == 0) return value;
                    else return ((value / unit) + 1) * unit;
                }

                public long Length
                {
                    get
                    {
                        return _length;
                    }
                }

                public void SetLength(long length)
                {
                    {
                        long size = BitmapManager.Roundup(length, 8);

                        _bitmapStream.SetLength(size);
                        _bitmapStream.Seek(0, SeekOrigin.Begin);

                        {
                            using (var safeBuffer = _bufferManager.CreateSafeBuffer(4096))
                            {
                                Unsafe.Zero(safeBuffer.Value);

                                for (long i = (size / safeBuffer.Value.Length), remain = size; i >= 0; i--, remain -= safeBuffer.Value.Length)
                                {
                                    _bitmapStream.Write(safeBuffer.Value, 0, (int)Math.Min(remain, safeBuffer.Value.Length));
                                    _bitmapStream.Flush();
                                }
                            }
                        }
                    }

                    _length = length;

                    {
                        _cacheChanged = false;
                        _cacheSector = -1;

                        _cacheBufferLength = 0;
                    }
                }

                private void Flush()
                {
                    if (_cacheChanged)
                    {
                        _bitmapStream.Seek(_cacheSector * SectorSize, SeekOrigin.Begin);
                        _bitmapStream.Write(_cacheBuffer, 0, _cacheBufferLength);
                        _bitmapStream.Flush();

                        _cacheChanged = false;
                    }
                }

                private ArraySegment<byte> GetBuffer(long sector)
                {
                    if (_cacheSector != sector)
                    {
                        this.Flush();

                        _bitmapStream.Seek(sector * SectorSize, SeekOrigin.Begin);
                        _cacheBufferLength = _bitmapStream.Read(_cacheBuffer, 0, _cacheBuffer.Length);

                        _cacheSector = sector;
                    }

                    return new ArraySegment<byte>(_cacheBuffer, 0, _cacheBufferLength);
                }

                public bool Get(long point)
                {
                    if (point >= _length) throw new ArgumentOutOfRangeException(nameof(point));

                    long sectorOffset = (point / 8) / SectorSize;
                    int bufferOffset = (int)((point / 8) % SectorSize);
                    byte bitOffset = (byte)(point % 8);

                    var buffer = this.GetBuffer(sectorOffset);
                    return ((buffer.Array[buffer.Offset + bufferOffset] << bitOffset) & 0x80) == 0x80;
                }

                public void Set(long point, bool state)
                {
                    if (point >= _length) throw new ArgumentOutOfRangeException(nameof(point));

                    long sectorOffset = (point / 8) / SectorSize;
                    int bufferOffset = (int)((point / 8) % SectorSize);
                    byte bitOffset = (byte)(point % 8);

                    if (state)
                    {
                        var buffer = this.GetBuffer(sectorOffset);
                        buffer.Array[buffer.Offset + bufferOffset] |= (byte)(0x80 >> bitOffset);
                    }
                    else
                    {
                        var buffer = this.GetBuffer(sectorOffset);
                        buffer.Array[buffer.Offset + bufferOffset] &= (byte)(~(0x80 >> bitOffset));
                    }

                    _cacheChanged = true;
                }

                protected override void Dispose(bool disposing)
                {
                    if (_disposed) return;
                    _disposed = true;

                    if (disposing)
                    {
                        if (_bitmapStream != null)
                        {
                            try
                            {
                                _bitmapStream.Dispose();
                            }
                            catch (Exception)
                            {

                            }

                            _bitmapStream = null;
                        }

                        if (_cacheBuffer != null)
                        {
                            try
                            {
                                _bufferManager.ReturnBuffer(_cacheBuffer);
                            }
                            catch (Exception)
                            {

                            }

                            _cacheBuffer = null;
                        }
                    }
                }
            }
        }
    }
}
