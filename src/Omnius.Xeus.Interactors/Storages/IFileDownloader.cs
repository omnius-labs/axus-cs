using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.Storages
{
    public interface IFileDownloaderFactory
    {
        ValueTask<IFileDownloader> CreateAsync(FileDownloaderOptions options);
    }

    public class FileDownloaderOptions
    {
        public IXeusService? XeusService { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IFileDownloader : IAsyncDisposable
    {
    }
}
