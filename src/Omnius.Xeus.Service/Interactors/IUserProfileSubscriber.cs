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
        public UserProfileSubscriberOptions(string configDirectoryPath, IXeusService xeusService, IBytesPool bytesPool)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
            this.XeusService = xeusService;
            this.BytesPool = bytesPool;
        }

        public string? ConfigDirectoryPath { get; }

        public IXeusService? XeusService { get; }

        public IBytesPool? BytesPool { get; }
    }

    public interface IUserProfileSubscriber : IAsyncDisposable
    {
        ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default);

        ValueTask SubscribeAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask<XeusUserProfile?> GetUserProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
