using System;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal record WrittenDeclaredMessageItem
    {
        public WrittenDeclaredMessageItem(OmniSignature signature, DateTime creationTime)
        {
            this.Signature = signature;
            this.CreationTime = creationTime;
        }

        public OmniSignature Signature { get; }

        public DateTime CreationTime { get; }
    }
}
