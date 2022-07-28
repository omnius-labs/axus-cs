using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface IBarkSubscriber : IAsyncDisposable
{
    ValueTask<IEnumerable<BarkMessage>> FindByTagAsync(string tag, CancellationToken cancellationToken = default);

    ValueTask<BarkMessage?> FindByReplyAsync(string tag, BarkReply reply, CancellationToken cancellationToken = default);

    ValueTask<BarkSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(BarkSubscriberConfig config, CancellationToken cancellationToken = default);
}
