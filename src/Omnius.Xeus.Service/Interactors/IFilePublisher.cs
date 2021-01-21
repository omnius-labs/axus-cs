using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Interactors
{
    public interface IFilePublisherFactory
    {
        ValueTask<IFilePublisher> CreateAsync(FilePublisherOptions options);
    }

    public class FilePublisherOptions
    {
        public IXeusService? XeusService { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IFilePublisher : IAsyncDisposable
    {
        ValueTask PublishAsync(string filePath, CancellationToken cancellationToken = default);

        ValueTask UnpublishAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
