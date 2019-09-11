using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Omnix.Base;

namespace Xeus.Core.Internal.Search.Primitives
{
    internal static class RouteTableMethods
    {
        private static readonly byte[] _distanceHashTable = new byte[256];

        static RouteTableMethods()
        {
            _distanceHashTable[0] = 0;
            _distanceHashTable[1] = 1;

            int i = 2;

            for (; i < 0x4; i++) _distanceHashTable[i] = 2;
            for (; i < 0x8; i++) _distanceHashTable[i] = 3;
            for (; i < 0x10; i++) _distanceHashTable[i] = 4;
            for (; i < 0x20; i++) _distanceHashTable[i] = 5;
            for (; i < 0x40; i++) _distanceHashTable[i] = 6;
            for (; i < 0x80; i++) _distanceHashTable[i] = 7;
            for (; i <= 0xff; i++) _distanceHashTable[i] = 8;
        }

        public static int Distance(byte[] x, byte[] y)
        {
            int result = 0;

            int length = Math.Min(x.Length, y.Length);

            for (int i = 0; i < length; i++)
            {
                byte value = (byte)(x[i] ^ y[i]);

                result = _distanceHashTable[value];

                if (result != 0)
                {
                    result += (length - (i + 1)) * 8;

                    break;
                }
            }

            return result;
        }

        public static IEnumerable<NodeInfo<T>> Search<T>(ReadOnlySpan<byte> baseId, ReadOnlySpan<byte> targetId, IEnumerable<NodeInfo<T>> nodeList, int count)
            where T : notnull
        {
            if (targetId == null) throw new ArgumentNullException(nameof(targetId));
            if (nodeList == null) throw new ArgumentNullException(nameof(nodeList));
            if (count == 0) return Array.Empty<NodeInfo<T>>();

            var targetList = new List<SortInfo<T>>();

            if (baseId != null)
            {
                var xor = new byte[targetId.Length];
                BytesOperations.Xor(targetId, baseId, xor);
                targetList.Add(new SortInfo<T>(null, xor));
            }

            foreach (var node in nodeList)
            {
                var xor = new byte[targetId.Length];
                BytesOperations.Xor(targetId, node.Id, xor);
                targetList.Add(new SortInfo<T>(node, xor));
            }

            for (int i = 1; i < targetList.Count; i++)
            {
                var temp = targetList[i];

                int left = 0;
                int right = Math.Min(i, count);

                while (left < right)
                {
                    int middle = (left + right) / 2;

                    if (BytesOperations.Compare(targetList[middle].Xor, temp.Xor) <= 0)
                    {
                        left = middle + 1;
                    }
                    else
                    {
                        right = middle;
                    }
                }

                for (int j = Math.Min(i, count); left < j; --j)
                {
                    targetList[j] = targetList[j - 1];
                }

                targetList[left] = temp;
            }

            return targetList.Take(count).TakeWhile(n => n.Node.HasValue).Select(n => n.Node!.Value).ToList();
        }

        private readonly struct SortInfo<T>
            where T : notnull
        {
            private readonly NodeInfo<T>? _node;
            private readonly byte[] _xor;

            public SortInfo(NodeInfo<T>? node, byte[] xor)
            {
                _node = node;
                _xor = xor;
            }

            public NodeInfo<T>? Node => _node;
            public byte[] Xor => _xor;
        }
    }
}
