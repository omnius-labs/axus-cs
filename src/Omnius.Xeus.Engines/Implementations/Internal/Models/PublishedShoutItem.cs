using System;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Internal.Models
{
    internal record PublishedShoutItem
    {
        public PublishedShoutItem(OmniSignature signature, DateTime creationTime, string registrant)
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
