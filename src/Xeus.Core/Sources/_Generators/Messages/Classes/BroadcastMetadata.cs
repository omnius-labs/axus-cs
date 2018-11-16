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
    sealed partial class BroadcastMetadata : MessageBase<BroadcastMetadata>
    {
        public static BroadcastMetadata Create(string type, DateTime creationTime, Metadata metadata, DigitalSignature digitalSignature)
        {
            var target = new BroadcastMetadata(type, creationTime, metadata, (Certificate)null);
            var certificate = Certificate.Create(digitalSignature, target.Export(BufferManager.Instance));

            return new BroadcastMetadata(type, creationTime, metadata, certificate);
        }

        public bool VerifyCertificate()
        {
            var target = new BroadcastMetadata(this.Type, this.CreationTime, this.Metadata, (Certificate)null);
            return this.Certificate.Verify(target.Export(BufferManager.Instance));
        }
    }
}
