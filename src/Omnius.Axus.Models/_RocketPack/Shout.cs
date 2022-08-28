using System.Buffers;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;

namespace Omnius.Axus.Models;

public sealed partial class Shout
{
    public static Shout Create(string channel, Timestamp64 createdTime, IMemoryOwner<byte> value, OmniDigitalSignature digitalSignature)
    {
        using var bytesPipe = new BytesPipe();
        var target = new Shout(channel, createdTime, value, null);
        target.Export(bytesPipe.Writer, BytesPool.Shared);

        var certificate = OmniDigitalSignature.CreateOmniCertificate(digitalSignature, bytesPipe.Reader.GetSequence());
        return new Shout(channel, createdTime, value, certificate);
    }

    public bool Verify()
    {
        using var bytesPipe = new BytesPipe();
        var target = new Shout(this.Channel, this.CreatedTime, this.Value, null);
        target.Export(bytesPipe.Writer, BytesPool.Shared);

        return this.Certificate?.Verify(bytesPipe.Reader.GetSequence()) ?? false;
    }
}
