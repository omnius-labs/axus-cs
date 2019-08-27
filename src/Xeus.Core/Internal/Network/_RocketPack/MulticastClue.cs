using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Algorithms.Cryptography;
using Omnix.Base;
using Omnix.Io;
using Omnix.Serialization;
using Omnix.Serialization.RocketPack;
using Xeus.Messages;

namespace Xeus.Core.Internal.Exchange
{
    sealed partial class MulticastClue
    {
        private static readonly Lazy<UTF8Encoding> _utf8Encoding = new Lazy<UTF8Encoding>(() => new UTF8Encoding(false));

        public static async ValueTask<MulticastClue> Create(string type, OmniSignature signature, Timestamp creationTime, XeusClue clue, OmniDigitalSignature digitalSignature, int miningLimit, TimeSpan miningTimeout, CancellationToken token)
        {
            OmniHashcash? hashcash = null;
            {
                using var hub = new Hub();

                var target = new MulticastClue(type, signature, creationTime, clue, null, null);
                target.Export(hub.Writer, BufferPool.Shared);
                hub.Writer.Complete();

                hashcash = await OmniMiner.Create(hub.Reader.GetSequence(), _utf8Encoding.Value.GetBytes(digitalSignature.GetOmniSignature().ToString()), OmniHashcashAlgorithmType.Simple_Sha2_256, miningLimit, miningTimeout, token);
                hub.Reader.Complete();
            }

            {
                using var hub = new Hub();

                var target = new MulticastClue(type, signature, creationTime, clue, hashcash, null);
                target.Export(hub.Writer, BufferPool.Shared);
                hub.Writer.Complete();

                var certificate = OmniCertificate.Create(digitalSignature, hub.Reader.GetSequence());
                hub.Reader.Complete();

                return new MulticastClue(type, signature, creationTime, clue, hashcash, certificate);
            }
        }

        public bool VerifyCertificate()
        {
            if (this.Certificate is null) return false;

            using var hub = new Hub();

            var target = new MulticastClue(this.Type, this.Signature, this.CreationTime, this.Clue, this.Hashcash, null);
            target.Export(hub.Writer, BufferPool.Shared);
            hub.Writer.Complete();

            var result = this.Certificate.Verify(hub.Reader.GetSequence());
            hub.Reader.Complete();

            return result;
        }

        private uint? _cost;

        public uint Cost
        {
            get
            {
                if (_cost == null)
                {
                    if (this.Hashcash == null || this.Certificate == null)
                    {
                        _cost = 0;
                    }
                    else
                    {
                        using var hub = new Hub();

                        var target = new MulticastClue(this.Type, this.Signature, this.CreationTime, this.Clue, null, null);
                        target.Export(hub.Writer, BufferPool.Shared);
                        hub.Writer.Complete();

                        _cost = OmniMiner.Verify(this.Hashcash, hub.Reader.GetSequence(), _utf8Encoding.Value.GetBytes(this.Certificate.GetOmniSignature().ToString()));
                        hub.Reader.Complete();
                    }
                }

                return _cost.Value;
            }
        }
    }
}
