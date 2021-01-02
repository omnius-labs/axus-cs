using System;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal sealed class PushDeclaredMessageStatus
    {
        public PushDeclaredMessageStatus(OmniSignature signature, DateTime creationTime)
        {
            this.Signature = signature;
            this.CreationTime = creationTime;
        }

        public OmniSignature Signature { get; }

        public DateTime CreationTime { get; }
    }
}
