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
}
