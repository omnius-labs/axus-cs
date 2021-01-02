using System;
using System.Buffers;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

#nullable enable

namespace Omnius.Xeus.Engines.Models
{
    public sealed partial class DeclaredMessage
    {
        public static DeclaredMessage Create(Timestamp creationTime, IMemoryOwner<byte> value, OmniDigitalSignature digitalSignature)
        {
            using var hub = new BytesHub();
            var target = new DeclaredMessage(creationTime, value, null);
            target.Export(hub.Writer, BytesPool.Shared);

            var certificate = OmniDigitalSignature.CreateOmniCertificate(digitalSignature, hub.Reader.GetSequence());
            return new DeclaredMessage(creationTime, value, certificate);
        }

        public bool Verify()
        {
            using var hub = new BytesHub();
            var target = new DeclaredMessage(this.CreationTime, _value, null);
            target.Export(hub.Writer, BytesPool.Shared);

            return this.Certificate?.Verify(hub.Reader.GetSequence()) ?? false;
        }
    }
}
