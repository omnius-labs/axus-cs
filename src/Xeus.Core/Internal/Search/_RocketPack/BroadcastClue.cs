using Omnix.Algorithms.Cryptography;
using Omnix.Base;
using Omnix.Serialization;
using Omnix.Serialization.RocketPack;
using Xeus.Messages;

namespace Xeus.Core.Internal.Search
{
    sealed partial class BroadcastClue
    {
        public static BroadcastClue Create(string type, Timestamp creationTime, XeusClue clue, OmniDigitalSignature digitalSignature)
        {
            using var hub = new Hub();

            var target = new BroadcastClue(type, creationTime, clue, null);
            target.Export(hub.Writer, BufferPool.Shared);
            hub.Writer.Complete();

            var certificate = OmniCertificate.Create(digitalSignature, hub.Reader.GetSequence());
            hub.Reader.Complete();

            return new BroadcastClue(type, creationTime, clue, certificate);
        }

        public bool VerifyCertificate()
        {
            if (this.Certificate is null) return false;

            using var hub = new Hub();

            var target = new BroadcastClue(this.Type, this.CreationTime, this.Clue, null);
            target.Export(hub.Writer, BufferPool.Shared);
            hub.Writer.Complete();

            var result = this.Certificate.Verify(hub.Reader.GetSequence());
            hub.Reader.Complete();

            return result;
        }
    }
}
