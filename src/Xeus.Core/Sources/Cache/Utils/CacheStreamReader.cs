using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amoeba.Messages;
using Omnius.Base;

namespace Amoeba.Service
{
    partial class CacheManager
    {
        public sealed class CacheStreamReader : Stream
        {
            private List<Hash> _hashes = new List<Hash>();
            private int _hashesIndex;

            private CacheManager _cacheManager;
            private BufferManager _bufferManager;

            private ArraySegment<byte> _blockBuffer;
            private int _blockBufferPosition = -1;

            private long _position;
            private long _length;

            private volatile bool _disposed;

            public CacheStreamReader(IEnumerable<Hash> hashes, CacheManager cacheManager, BufferManager bufferManager)
            {
                _hashes.AddRange(hashes);
                _cacheManager = cacheManager;
                _bufferManager = bufferManager;

                _length = _hashes.Sum(n => (long)cacheManager.GetLength(n));
            }

            public override bool CanRead
            {
                get
                {
                    if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                    return true;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                    return true;
                }
            }

            public override long Position
            {
                get
                {
                    if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                    return _position;
                }
                set
                {
                    if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                    if (_position != value) throw new NotSupportedException();
                }
            }

            public override long Length
            {
                get
                {
                    if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                    return _length;
                }
            }

            public override long Seek(long offset, System.IO.SeekOrigin origin)
            {
                if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);
                if (offset < 0 || buffer.Length < offset) throw new ArgumentOutOfRangeException(nameof(offset));
                if (count < 0 || (buffer.Length - offset) < count) throw new ArgumentOutOfRangeException(nameof(count));

                count = (int)Math.Min(count, this.Length - this.Position);

                int readLength = 0;

                while (count > 0)
                {
                    if (_blockBufferPosition == -1)
                    {
                        _blockBuffer = _cacheManager.GetBlock(_hashes[_hashesIndex++]);
                        _blockBufferPosition = 0;
                    }

                    int length = Math.Min(count, _blockBuffer.Count - _blockBufferPosition);
                    Unsafe.Copy(_blockBuffer.Array, _blockBuffer.Offset + _blockBufferPosition, buffer, offset, length);
                    _blockBufferPosition += length;
                    count -= length;
                    offset += length;
                    readLength += length;

                    if (_blockBuffer.Count == _blockBufferPosition)
                    {
                        if (_blockBuffer.Array != null)
                        {
                            _bufferManager.ReturnBuffer(_blockBuffer.Array);
                            _blockBuffer = default;
                        }
                        _blockBufferPosition = -1;
                    }
                }

                _position += readLength;
                return readLength;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                throw new NotSupportedException();
            }

            public override void Flush()
            {
                if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (_disposed) return;
                    _disposed = true;

                    if (disposing)
                    {
                        if (_blockBuffer.Array != null)
                        {
                            _bufferManager.ReturnBuffer(_blockBuffer.Array);
                            _blockBuffer = default;
                        }
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
    }
}
