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
        public FileSubscriberOptions(IXeusService xeusService, IBytesPool bytesPool)
        {
            this.XeusService = xeusService;
            this.BytesPool = bytesPool;
        }

        public IXeusService XeusService { get; }

        public IBytesPool BytesPool { get; }
    }

    public interface IFileSubscriber : IAsyncDisposable
    {
        ValueTask SubscribeAsync(OmniHash contentHash, CancellationToken cancellationToken = default);

        ValueTask UnsubscribeAsync(OmniHash contentHash, CancellationToken cancellationToken = default);
    }
}
