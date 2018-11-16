using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using Amoeba.Messages;
using Omnius.Base;
using Omnius.Io;
using Omnius.Security;
using Omnius.Serialization;
using Omnius.Utils;

namespace Amoeba.Service
{
    sealed partial class MulticastMetadata : MessageBase<MulticastMetadata>
    {
        public MulticastMetadata(string type, Tag tag, DateTime creationTime, Metadata metadata, DigitalSignature digitalSignature, Miner miner, CancellationToken token)
        {
            this.Type = type;
            this.Tag = tag;
            this.CreationTime = creationTime;
            this.Metadata = metadata;
            this.Cash = miner.Create(this.GetCashStream(digitalSignature.ToString()), token).Result;
            this.Certificate = new Certificate(digitalSignature, this.GetCertificateStream());
        }

        public Stream GetCashStream(string signature)
        {
            var bufferManager = BufferManager.Instance;

            var signatureStream = new RecyclableMemoryStream(bufferManager);
            {
                var writer = new MessageStreamWriter(signatureStream, bufferManager);
                writer.Write((ulong)5);
                writer.Write(signature);

                signatureStream.Seek(0, SeekOrigin.Begin);
            }

            var target = new MulticastMetadata(this.Type, this.Tag, this.CreationTime, this.Metadata, (Cash)null, (Certificate)null);

            return new UniteStream(target.Export(bufferManager), signatureStream);
        }

        public Stream GetCertificateStream()
        {
            var target = new MulticastMetadata(this.Type, this.Tag, this.CreationTime, this.Metadata, this.Cash, (Certificate)null);
            return target.Export(BufferManager.Instance);
        }

        public bool VerifyCertificate()
        {
            return this.Certificate.Verify(this.GetCertificateStream());
        }

        private volatile Cost _cost;

        public Cost Cost
        {
            get
            {
                if (_cost == null)
                {
                    if (this.Cash == null || this.Certificate == null)
                    {
                        _cost = new Cost(CashAlgorithm.None, 0);
                    }
                    else
                    {
                        using (var stream = this.GetCashStream(this.Certificate.ToString()))
                        {
                            _cost = Miner.Verify(this.Cash, stream);
                        }
                    }
                }

                return _cost;
            }
        }
    }
}
