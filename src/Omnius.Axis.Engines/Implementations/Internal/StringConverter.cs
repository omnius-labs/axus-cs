using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Pipelines;
using Omnius.Core.Serialization;

namespace Omnius.Axis.Engines.Internal;

internal static class StringConverter
{
    public static string HashToString(OmniHash hash)
    {
        return hash.ToString(ConvertStringType.Base16);
    }

    public static string SignatureToString(OmniSignature signature)
    {
        using var bytesPipe = new BytesPipe(BytesPool.Shared);
        signature.Export(bytesPipe.Writer, BytesPool.Shared);
        var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bytesPipe.Reader.GetSequence()));
        return hash.ToString(ConvertStringType.Base16);
    }
}
