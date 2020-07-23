using System;
using System.Net.Http;
using System.Buffers;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Core.Serialization.RocketPack;

#nullable enable

namespace Omnius.Xeus.Service.Models
{
    public sealed partial class DeclaredMessage
    {
        public static DeclaredMessage Create(IMemoryOwner<byte> value, OmniDigitalSignature digitalSignature)
        {
            var creationTime = Timestamp.FromDateTime(DateTime.UtcNow);
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
