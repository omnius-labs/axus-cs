using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Interactors
{
    public interface IFileDownloaderFactory
    {
        ValueTask<IFileDownloader> CreateAsync(FileDownloaderOptions options, IXeusService xeusService, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IFileDownloader : IAsyncDisposable
    {
        ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask RegisterAsync(XeusFileMeta fileMeta, string targetDirectoryPath, CancellationToken cancellationToken = default);

        ValueTask UnregisterAsync(XeusFileMeta fileMeta, string targetDirectoryPath, CancellationToken cancellationToken = default);
    }
}
