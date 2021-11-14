using Omnius.Core.Cryptography;
using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors;

public interface IProfilePublisher
{
    ValueTask<IEnumerable<PublishedProfileReport>> GetPublishedProfileReportsAsync(CancellationToken cancellationToken = default);

    ValueTask RegisterAsync(ProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default);

    ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default);
}
