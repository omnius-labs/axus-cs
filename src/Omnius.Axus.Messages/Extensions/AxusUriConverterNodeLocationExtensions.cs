using System.Diagnostics.CodeAnalysis;

namespace Omnius.Axus.Messages;

public static class AxusUriConverterNodeLocationExtensions
{
    public static string NodeLocationToString(this AxusUriConverter converter, NodeLocation nodeLocation)
    {
        return converter.Encode<NodeLocation>("node", 1, nodeLocation);
    }

    public static bool TryStringToNodeLocation(this AxusUriConverter converter, string text, [NotNullWhen(true)] out NodeLocation? nodeLocation)
    {
        nodeLocation = null;
        return converter.TryDecode("node", 1, text, out nodeLocation);
    }
}
