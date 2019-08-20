using System;
using System.IO;
using Omnix.Algorithms.Cryptography;
using Omnix.Base;
using Omnix.Serialization;
using Omnix.Serialization.RocketPack;
using Xeus.Messages;

namespace Xeus.Core.Internal.Exchange
{
    sealed partial class UnicastClue
    {
        public static UnicastClue Create(string type, OmniSignature signature, Timestamp creationTime, XeusClue clue, OmniDigitalSignature digitalSignature)
        {
            using var hub = new Hub();

            var target = new UnicastClue(type, signature, creationTime, clue, null);
            target.Export(hub.Writer, BufferPool.Shared);
            hub.Writer.Complete();

            var certificate = OmniCertificate.Create(digitalSignature, hub.Reader.GetSequence());
            hub.Reader.Complete();

            return new UnicastClue(type, signature, creationTime, clue, certificate);
        }

        public bool VerifyCertificate()
        {
            if (this.Certificate is null) return false;

            using var hub = new Hub();

            var target = new UnicastClue(this.Type, this.Signature, this.CreationTime, this.Clue, null);
            target.Export(hub.Writer, BufferPool.Shared);
            hub.Writer.Complete();

            var result = this.Certificate.Verify(hub.Reader.GetSequence());
            hub.Reader.Complete();

            return result;
        }
    }
}
