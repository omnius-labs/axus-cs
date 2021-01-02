using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.Storages
{
    public interface IProfileSubscriberFactory
    {
        ValueTask<IProfileSubscriber> CreateAsync(ProfileSubscriberOptions options);
    }

    public class ProfileSubscriberOptions
    {
        public string? ConfigDirectoryPath { get; init; }

        public IXeusService? XeusService { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IProfileSubscriber
    {
        ValueTask RegisterSignaturesAsync(IEnumerable<OmniSignature> signatures, CancellationToken cancellationToken = default);

        ValueTask UnregisterSignaturesAsync(IEnumerable<OmniSignature> signatures, CancellationToken cancellationToken = default);

        ValueTask<ProfileSubscriberGetProfileResult> GetProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }

    public struct ProfileSubscriberGetProfileResult
    {
        public DateTime CreationTime { get; init; }

        public XeusProfile Profile { get; init; }
    }
}
