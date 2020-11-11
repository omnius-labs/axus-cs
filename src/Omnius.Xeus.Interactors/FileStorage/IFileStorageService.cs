using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.FileStorage
{
    public interface IFileStorageServiceFactory
    {
        ValueTask<IFileStorageService> CreateAsync(FileStorageServiceOptions options, IXeusService xeusService, IBytesPool bytesPool);
    }

    public interface IFileStorageService : IAsyncDisposable
    {
    }
}
