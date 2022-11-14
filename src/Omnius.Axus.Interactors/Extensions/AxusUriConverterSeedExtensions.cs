using System.Diagnostics.CodeAnalysis;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;

namespace Omnius.Axus.Interactors;

public static class AxusUriConverterSeedExtensions
{
    public static string SeedToString(this AxusUriConverter converter, Seed seed)
    {
        return converter.Encode<Seed>("seed", 1, seed);
    }

    public static bool TryStringToSeed(this AxusUriConverter converter, string text, [NotNullWhen(true)] out Seed? seed)
    {
        seed = null;
        return converter.TryDecode("seed", 1, text, out seed);
    }
}
