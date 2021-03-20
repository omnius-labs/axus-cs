using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Storages;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors
{
    public interface IUserProfileDownloaderFactory
    {
        ValueTask<IUserProfileDownloader> CreateAsync(UserProfileDownloaderOptions options, IXeusService xeusService,
            IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IUserProfileDownloader : IAsyncDisposable
    {
        ValueTask<IEnumerable<DownloadingUserProfileReport>> GetDownloadingUserProfileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask RegisterAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask<XeusUserProfile?> ExportAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
