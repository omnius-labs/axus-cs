using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Daemon;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
{
    public interface IFileDownloaderFactory
    {
        ValueTask<IFileDownloader> CreateAsync(FileDownloaderOptions options, IXeusService xeusService, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IFileDownloader : IAsyncDisposable
    {
        ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask RegisterAsync(Box box, string targetDirectoryPath, CancellationToken cancellationToken = default);

        ValueTask UnregisterAsync(Box box, string targetDirectoryPath, CancellationToken cancellationToken = default);
    }
}
