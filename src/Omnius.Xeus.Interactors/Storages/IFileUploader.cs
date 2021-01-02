using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.Storages
{
    public interface IFileUploaderFactory
    {
        ValueTask<IFileUploader> CreateAsync(FileUploaderOptions options);
    }

    public class FileUploaderOptions
    {
        public IXeusService? XeusService { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IFileUploader : IAsyncDisposable
    {
    }
}
