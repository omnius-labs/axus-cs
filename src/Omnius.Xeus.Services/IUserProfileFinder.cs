using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Storages;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
{
    public interface IUserProfileFinderFactory
    {
        ValueTask<IUserProfileFinder> CreateAsync(UserProfileFinderOptions options, IUserProfileDownloader userProfileDownloader,
            IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IUserProfileFinder : IAsyncDisposable
    {
        ValueTask<UserProfileFinderSearchOptions> GetSearchOptionsAsync(CancellationToken cancellationToken = default);

        ValueTask SetSearchOptionsAsync(UserProfileFinderSearchOptions searchOptions, CancellationToken cancellationToken = default);

        ValueTask<UserProfile?> FindOneAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        IAsyncEnumerable<UserProfile> FindAllAsync(CancellationToken cancellationToken = default);
    }
}
