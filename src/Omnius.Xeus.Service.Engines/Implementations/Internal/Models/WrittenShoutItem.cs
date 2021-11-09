using System;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Engines.Internal.Models;

internal record WrittenShoutItem
{
    public WrittenShoutItem(OmniSignature signature, DateTime creationTime)
    {
        this.Signature = signature;
        this.CreationTime = creationTime;
    }

    public OmniSignature Signature { get; }

    public DateTime CreationTime { get; }
}