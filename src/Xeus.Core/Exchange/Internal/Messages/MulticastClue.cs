using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Cryptography;
using Omnix.Io;
using Omnix.Serialization;
using Xeus.Messages;

namespace Xeus.Core.Exchange.Internal
{
    sealed partial class MulticastClue
    {
        private static readonly Lazy<UTF8Encoding> _utf8Encoding = new Lazy<UTF8Encoding>(() => new UTF8Encoding(false));

        public static async ValueTask<MulticastClue> Create(string type, Channel channel, Timestamp creationTime, Clue clue, OmniDigitalSignature digitalSignature, int miningLimit, TimeSpan miningTimeout, CancellationToken token)
        {
            OmniHashcash hashcash = null;
            {
                var hub = new Hub();

                try
                {
                    var target = new MulticastClue(type, channel, creationTime, clue, null, null);
                    target.Export(hub.Writer, BufferPool.Shared);
                    hub.Writer.Complete();

                    hashcash = await OmniMiner.Create(hub.Reader.GetSequence(), _utf8Encoding.Value.GetBytes(digitalSignature.GetOmniSignature().ToString()), OmniHashcashAlgorithmType.Sha2_256, miningLimit, miningTimeout, token);
                    hub.Reader.Complete();
                }
                finally
                {
                    hub.Reset();
                }
            }

            {
                var hub = new Hub();

                try
                {
                    var target = new MulticastClue(type, channel, creationTime, clue, hashcash, null);
                    target.Export(hub.Writer, BufferPool.Shared);
                    hub.Writer.Complete();

                    var certificate = OmniCertificate.Create(digitalSignature, hub.Reader.GetSequence());
                    hub.Reader.Complete();

                    return new MulticastClue(type, channel, creationTime, clue, hashcash, certificate);
                }
                finally
                {
                    hub.Reset();
                }
            }
        }

        public bool VerifyCertificate()
        {
            var hub = new Hub();

            try
            {
                var target = new MulticastClue(this.Type, this.Channel, this.CreationTime, this.Clue, this.Hashcash, null);
                target.Export(hub.Writer, BufferPool.Shared);
                hub.Writer.Complete();

                var result = this.Certificate.Verify(hub.Reader.GetSequence());
                hub.Reader.Complete();

                return result;
            }
            finally
            {
                hub.Reset();
            }
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
                        var hub = new Hub();

                        try
                        {
                            var target = new MulticastClue(this.Type, this.Channel, this.CreationTime, this.Clue, null, null);
                            target.Export(hub.Writer, BufferPool.Shared);
                            hub.Writer.Complete();

                            _cost = OmniMiner.Verify(this.Hashcash, hub.Reader.GetSequence(), _utf8Encoding.Value.GetBytes(this.Certificate.GetOmniSignature().ToString()));
                            hub.Reader.Complete();
                        }
                        finally
                        {
                            hub.Reset();
                        }
                    }
                }

                return _cost.Value;
            }
        }
    }
}
