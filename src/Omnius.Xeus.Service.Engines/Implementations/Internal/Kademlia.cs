using Omnius.Core;

namespace Omnius.Xeus.Service.Engines.Internal;

internal readonly struct KademliaElement<T>
{
    public KademliaElement(ReadOnlyMemory<byte> id, T value)
    {
        this.Id = id;
        this.Value = value;
    }

    public ReadOnlyMemory<byte> Id { get; }

    public T Value { get; }
}

internal static class Kademlia
{
    private static readonly byte[] _distanceHashTable = new byte[256];

    static Kademlia()
    {
        _distanceHashTable[0] = 0;
        _distanceHashTable[1] = 1;

        int i = 2;

        for (; i < 0x4; i++)
        {
            _distanceHashTable[i] = 2;
        }

        for (; i < 0x8; i++)
        {
            _distanceHashTable[i] = 3;
        }

        for (; i < 0x10; i++)
        {
            _distanceHashTable[i] = 4;
        }

        for (; i < 0x20; i++)
        {
            _distanceHashTable[i] = 5;
        }

        for (; i < 0x40; i++)
        {
            _distanceHashTable[i] = 6;
        }

        for (; i < 0x80; i++)
        {
            _distanceHashTable[i] = 7;
        }

        for (; i <= 0xff; i++)
        {
            _distanceHashTable[i] = 8;
        }
    }

    public static int Distance(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
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

    public static IEnumerable<KademliaElement<T>> Search<T>(ReadOnlySpan<byte> baseId, ReadOnlySpan<byte> targetId, IEnumerable<KademliaElement<T>> elements, int count)
        where T : notnull
    {
        if (elements is null) throw new ArgumentNullException(nameof(elements));

        if (count == 0) Array.Empty<KademliaElement<T>>();

        var targetList = new List<SortEntry<T>>();

        if (baseId != null)
        {
            var xor = new byte[targetId.Length];
            BytesOperations.Xor(targetId, baseId, xor);
            targetList.Add(new SortEntry<T>(null, xor));
        }

        foreach (var node in elements)
        {
            var xor = new byte[targetId.Length];
            BytesOperations.Xor(targetId, node.Id.Span, xor);
            targetList.Add(new SortEntry<T>(node, xor));
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

    private readonly struct SortEntry<T>
    {
        public SortEntry(KademliaElement<T>? node, byte[] xor)
        {
            this.Node = node;
            this.Xor = xor;
        }

        public KademliaElement<T>? Node { get; }

        public byte[] Xor { get; }
    }
}
