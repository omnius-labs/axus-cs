using Omnius.Axis.Intaractors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Intaractors;

public interface IProfilePublisher
{
    ValueTask<IEnumerable<PublishedProfileReport>> GetPublishedProfileReportsAsync(CancellationToken cancellationToken = default);

    ValueTask RegisterAsync(ProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default);

    ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default);
}
