using System.Diagnostics.CodeAnalysis;
using Omnius.Axus.Models;
using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public static class AxusUriConverterFileSeedExtensions
{
    public static string FileSeedToString(this AxusUriConverter converter, FileSeed nodeLocation)
    {
        return converter.Encode<FileSeed>("node", 1, nodeLocation);
    }

    public static bool TryStringToFileSeed(this AxusUriConverter converter, string text, [NotNullWhen(true)] out FileSeed? nodeLocation)
    {
        nodeLocation = null;
        return converter.TryDecode("node", 1, text, out nodeLocation);
    }
}
