using System.Text;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Pipelines;
using Omnius.Core.Serialization;

namespace Omnius.Axus.Engines.Internal;

internal static class StringConverter
{
    private static readonly Lazy<Encoding> _encoding = new Lazy<Encoding>(() => new UTF8Encoding(false));

    public static string ToString(OmniHash hash)
    {
        return hash.ToString(ConvertStringType.Base16Lower);
    }

    public static string ToString(OmniSignature signature, string channel)
    {
        using var bytesPipe = new BytesPipe(BytesPool.Shared);
        _encoding.Value.GetBytes(signature.ToString(), bytesPipe.Writer);
        _encoding.Value.GetBytes("/", bytesPipe.Writer);
        _encoding.Value.GetBytes(channel, bytesPipe.Writer);

        var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bytesPipe.Reader.GetSequence()));
        return hash.ToString(ConvertStringType.Base16Lower);
    }
}
