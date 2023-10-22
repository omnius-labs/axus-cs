using System.Text;
using Omnius.Axus.Engine.Internal.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Engine.Internal.Services.Helpers;

internal static class ContentKeyConverter
{
    private static readonly Lazy<Encoding> _encoding = new Lazy<Encoding>(() => new UTF8Encoding(false));

    public static ContentKey ToContentKey(string schema, OmniHash hash)
    {
        return new ContentKey(schema, hash);
    }

    public static ContentKey ToContentKey(string schema, OmniSignature signature, string channel)
    {
        using var bytesPipe = new BytesPipe(BytesPool.Shared);
        _encoding.Value.GetBytes(signature.ToString(), bytesPipe.Writer);
        _encoding.Value.GetBytes("/", bytesPipe.Writer);
        _encoding.Value.GetBytes(channel, bytesPipe.Writer);

        var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bytesPipe.Reader.GetSequence()));
        return new ContentKey(schema, hash);
    }
}
