using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
{
    public interface IFileFinderFactory
    {
        ValueTask<IFileFinder> CreateAsync(FileFinderOptions options, IUserProfileFinder userProfileFinder, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IFileFinder : IAsyncDisposable
    {
        IAsyncEnumerable<Box> FindAllAsync(CancellationToken cancellationToken = default);
    }
}
