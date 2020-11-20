using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.Storages
{
    public interface IFileStorageFactory
    {
        ValueTask<IFileStorage> CreateAsync(FileStorageOptions options, IXeusService xeusService, IBytesPool bytesPool);
    }

    public interface IFileStorage : IAsyncDisposable
    {
    }
}
