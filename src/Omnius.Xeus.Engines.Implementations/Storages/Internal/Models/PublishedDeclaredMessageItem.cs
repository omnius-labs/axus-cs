using System;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal record PublishedDeclaredMessageItem
    {
        public PublishedDeclaredMessageItem(OmniSignature signature, DateTime creationTime, string registrant)
        {
            this.Signature = signature;
            this.CreationTime = creationTime;
            this.Registrant = registrant;
        }

        public OmniSignature Signature { get; }

        public DateTime CreationTime { get; }

        public string Registrant { get; }
    }
}
