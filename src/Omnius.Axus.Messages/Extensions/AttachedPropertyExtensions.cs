using System.Diagnostics.CodeAnalysis;
using Omnius.Core.RocketPack;

namespace Omnius.Axus.Messages;

public static class AttachedPropertyExtensions
{
    public static bool TryGetValue(this IEnumerable<AttachedProperty> properties, string name, out ReadOnlyMemory<byte> value)
    {
        foreach (var p in properties)
        {
            if (p.Name == name)
            {
                value = p.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public static bool TryGetValue<T>(this IEnumerable<AttachedProperty> properties, string name, [MaybeNullWhen(false)] out T value)
        where T : IRocketMessage<T>
    {
        foreach (var p in properties)
        {
            if (p.Name == name)
            {
                value = RocketMessage.FromBytes<T>(p.Value);
                return true;
            }
        }

        value = default;
        return false;
    }
}
