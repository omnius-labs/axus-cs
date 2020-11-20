using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.Storages
{
    public interface IUserProfileStorageFactory
    {
        ValueTask<IUserProfileStorage> CreateAsync(UserProfileStorageOptions options, IXeusService xeusService, IBytesPool bytesPool);
    }

    public interface IUserProfileStorage : IAsyncDisposable
    {
    }
}
