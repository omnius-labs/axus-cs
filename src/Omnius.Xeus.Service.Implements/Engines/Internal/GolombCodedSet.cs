using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using Omnius.Core.Extensions;

namespace Omnius.Xeus.Service.Engines.Internal
{
    internal sealed partial class GolombCodedSet<T>
    {
        private readonly Func<T, uint> _hashFunction;
        private readonly int _falsePositiveRate;
        private readonly List<uint> _list = new List<uint>();

        public GolombCodedSet(IEnumerable<T> collection, int falsePositiveRate, Func<T, uint> hashFunction)
        {
            _hashFunction = hashFunction;
            _falsePositiveRate = falsePositiveRate;

            var itemCount = collection.Count();

            foreach (var item in collection)
            {
                var hash = _hashFunction(item) % (uint)(itemCount * _falsePositiveRate);
                _list.Add(hash);
            }

            _list.Sort();
        }

        private GolombCodedSet(ReadOnlySequence<byte> sequence, int falsePositiveRate, Func<T, uint> hashFunction)
        {
            _hashFunction = hashFunction;
            _falsePositiveRate = falsePositiveRate;

            var decoder = new GolombDecoder(sequence, _falsePositiveRate);

            uint current = 0;

            while (decoder.TryDecode(out var diff))
            {
                current += diff;
                _list.Add(current);
            }
        }

        public static GolombCodedSet<T> Import(ReadOnlySequence<byte> sequence, int falsePositiveRate, Func<T, uint> computeHash)
        {
            return new GolombCodedSet<T>(sequence, falsePositiveRate, computeHash);
        }

        public void Export(IBufferWriter<byte> bufferWriter)
        {
            var encoder = new GolombEncoder(bufferWriter, _falsePositiveRate);

            uint previous = 0;

            foreach (var hash in _list)
            {
                encoder.Encoding(hash - previous);
                previous = hash;
            }

            encoder.Flush();
        }

        public bool Contains(T item)
        {
            var hash = _hashFunction(item) % (uint)(_list.Count * _falsePositiveRate);
            return _list.BinarySearch(hash) >= 0;
        }

        private static uint Bitmask(int n)
        {
            return ((uint)1 << n) - 1;
        }

        private static int FloorLog2(int v)
        {
            return sizeof(int) * 8 - LeadingZeros(v - 1);
        }

        private static int LeadingZeros(int x)
        {
            const int numIntBits = sizeof(int) * 8; //compile time constant
                                                    //do the smearing
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            //count the ones
            x -= x >> 1 & 0x55555555;
            x = (x >> 2 & 0x33333333) + (x & 0x33333333);
            x = (x >> 4) + x & 0x0f0f0f0f;
            x += x >> 8;
            x += x >> 16;
            return numIntBits - (x & 0x0000003f); //subtract # of 1s from 32
        }
    }
}
