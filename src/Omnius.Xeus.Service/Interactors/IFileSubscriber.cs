using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;

namespace Omnius.Xeus.Service.Interactors
{
    public interface IFileSubscriberFactory
    {
        ValueTask<IFileSubscriber> CreateAsync(FileSubscriberOptions options);
    }

    public class FileSubscriberOptions
    {
        public IXeusService? XeusService { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IFileSubscriber : IAsyncDisposable
    {
        ValueTask SubscribeAsync(OmniHash contentHash, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeAsync(OmniHash contentHash, CancellationToken cancellationToken = default);
    }
}
