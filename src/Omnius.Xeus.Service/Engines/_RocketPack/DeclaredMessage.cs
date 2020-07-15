using System.Net.Http;
using System.Buffers;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Drivers;

#nullable enable

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class DeclaredMessage
    {
        public static DeclaredMessage Create(IMemoryOwner<byte> value, OmniDigitalSignature digitalSignature)
        {
            using var hub = new BytesHub();
            var target = new DeclaredMessage(value, null);
            target.Export(hub.Writer, BytesPool.Shared);

            var certificate = OmniDigitalSignature.CreateOmniCertificate(digitalSignature, hub.Reader.GetSequence());
            return new DeclaredMessage(value, certificate);
        }

        public bool Verify()
        {
            using var hub = new BytesHub();
            var target = new DeclaredMessage(_value, null);
            target.Export(hub.Writer, BytesPool.Shared);

            return this.Certificate?.Verify(hub.Reader.GetSequence()) ?? false;
        }
    }
}
