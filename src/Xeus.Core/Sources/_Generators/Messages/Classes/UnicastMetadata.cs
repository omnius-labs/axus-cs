using System;
using System.IO;
using System.Runtime.Serialization;
using Amoeba.Messages;
using Omnius.Base;
using Omnius.Security;
using Omnius.Serialization;
using Omnius.Utils;

namespace Amoeba.Service
{
    sealed partial class UnicastMetadata : MessageBase<UnicastMetadata>
    {
        public UnicastMetadata(string type, Signature signature, DateTime creationTime, Metadata metadata, DigitalSignature digitalSignature)
        {
            this.Type = type;
            this.Signature = signature;
            this.CreationTime = creationTime;
            this.Metadata = metadata;
            this.Certificate = new Certificate(digitalSignature, this.GetCertificateStream());
        }

        public Stream GetCertificateStream()
        {
            var target = new UnicastMetadata(this.Type, this.Signature, this.CreationTime, this.Metadata, (Certificate)null);
            return target.Export(BufferManager.Instance);
        }

        public bool VerifyCertificate()
        {
            return this.Certificate.Verify(this.GetCertificateStream());
        }
    }
}
