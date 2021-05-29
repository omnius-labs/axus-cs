using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Daemon;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
{
    public interface IFileUploaderFactory
    {
        ValueTask<IFileUploader> CreateAsync(FileUploaderOptions options, IXeusService xeusService, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IFileUploader : IAsyncDisposable
    {
        ValueTask<IEnumerable<UploadingFileReport>> GetUploadingFileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask RegisterAsync(string filePath, CancellationToken cancellationToken = default);

        ValueTask UnregisterAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
