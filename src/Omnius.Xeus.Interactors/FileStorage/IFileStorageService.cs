using System;
using System.Threading.Tasks;
using Omnius.Xeus.Api;

namespace Omnius.Xeus.Interactors.FileStorage
{
    public interface IFileStorageServiceFactory
    {
        public ValueTask<IFileStorageService> CreateAsync(IXeusService xeusService);
    }

    public interface IFileStorageService : IAsyncDisposable
    {

    }
}
