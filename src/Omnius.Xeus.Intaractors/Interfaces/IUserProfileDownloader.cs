using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors
{
    public interface IUserProfileDownloader
    {
        ValueTask<IEnumerable<DownloadingUserProfileReport>> GetDownloadingUserProfileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask RegisterAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask<UserProfile?> ExportAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
