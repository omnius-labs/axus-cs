using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Primitives;

namespace Omnius.Xeus.Engines.Storages
{
    public interface IDeclaredMessagePublisherFactory
    {
        ValueTask<IDeclaredMessagePublisher> CreateAsync(DeclaredMessagePublisherOptions options, IBytesPool bytesPool);
    }

    public interface IDeclaredMessagePublisher : IReadOnlyDeclaredMessages, IAsyncDisposable
    {
        ValueTask<DeclaredMessagePublisherReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask PublishMessageAsync(DeclaredMessage message, string registrant, CancellationToken cancellationToken = default);

        ValueTask UnpublishMessageAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
    }
}
