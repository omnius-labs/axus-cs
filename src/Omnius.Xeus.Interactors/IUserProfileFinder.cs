using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Storages;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Interactors
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

        ValueTask<XeusUserProfile?> FindOneAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        IAsyncEnumerable<XeusUserProfile> FindAllAsync(CancellationToken cancellationToken = default);
    }
}
