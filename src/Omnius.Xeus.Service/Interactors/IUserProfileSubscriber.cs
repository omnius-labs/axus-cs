using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Interactors
{
    public interface IUserProfileSubscriberFactory
    {
        ValueTask<IUserProfileSubscriber> CreateAsync(UserProfileSubscriberOptions options);
    }

    public class UserProfileSubscriberOptions
    {
        public string? ConfigDirectoryPath { get; init; }

        public IXeusService? XeusService { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IUserProfileSubscriber
    {
        ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default);

        ValueTask SubscribeAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask<XeusUserProfile?> GetUserProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
