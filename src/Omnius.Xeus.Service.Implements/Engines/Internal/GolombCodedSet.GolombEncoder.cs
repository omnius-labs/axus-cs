using System;
using System.Buffers;

namespace Omnius.Xeus.Service.Engines.Internal
{
    partial class GolombCodedSet<T>
    {
        private sealed class GolombEncoder
        {
            private readonly BitWriter _bitWriter;
            private readonly int _p, _log2p;

            public GolombEncoder(IBufferWriter<byte> bufferWriter, int p)
            {
                _bitWriter = new BitWriter(bufferWriter);
                _p = p;
                _log2p = FloorLog2(p);
            }

            public void Encoding(uint value)
            {
                int q = (int)(value / _p);
                int r = (int)(value - (q * _p));

                _bitWriter.Write(q + 1, Bitmask(q) << 1);
                _bitWriter.Write(_log2p, (uint)r);
            }

            public void Flush()
            {
                _bitWriter.Flush();
            }

            private unsafe class BitWriter
            {
                private readonly IBufferWriter<byte> _bufferWriter;

                private ulong _bitBuffer;
                private int _bitBufferCount;
                private const int MaxBitBufferCount = sizeof(ulong) * 8;

                public BitWriter(IBufferWriter<byte> bufferWriter)
                {
                    _bufferWriter = bufferWriter;
                }

                public void Write(int nbits, uint value)
                {
                    while (nbits != 0)
                    {
                        int nb = Math.Min(MaxBitBufferCount - _bitBufferCount, nbits);
                        _bitBuffer <<= nb;
                        value &= Bitmask(nbits);
                        _bitBuffer |= value >> (nbits - nb);
                        _bitBufferCount += nb;
                        nbits -= nb;

                        int byteCount = (_bitBufferCount / 8);

                        if (byteCount != 0)
                        {
                            fixed (byte* buffer = _bufferWriter.GetSpan(byteCount))
                            {
                                for (int i = 0; i < byteCount; i++)
                                {
                                    buffer[i] = (byte)((_bitBuffer >> (_bitBufferCount - 8)) & Bitmask(8));
                                    _bitBufferCount -= 8;
                                    _bitBuffer &= Bitmask(_bitBufferCount);
                                }
                            }

                            _bufferWriter.Advance(byteCount);
                        }
                    }
                }

                public void Flush()
                {
                    if (_bitBufferCount > 0)
                    {
                        fixed (byte* buffer = _bufferWriter.GetSpan(1))
                        {
                            _bitBuffer <<= (8 - _bitBufferCount);
                            buffer[0] = (byte)(_bitBuffer & Bitmask(8));
                            _bitBufferCount = 0;
                            _bitBuffer = 0;
                        }

                        _bufferWriter.Advance(1);
                    }
                }
            }
        }
    }
}
