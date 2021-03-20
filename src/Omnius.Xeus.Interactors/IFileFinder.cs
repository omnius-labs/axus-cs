using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Interactors.Models;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Interactors
{
    public interface IFileFinderFactory
    {
        ValueTask<IFileFinder> CreateAsync(FileFinderOptions options, IUserProfileFinder userProfileFinder, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IFileFinder : IAsyncDisposable
    {
        IAsyncEnumerable<XeusFileMeta> FindAllAsync(CancellationToken cancellationToken = default);
    }
}
