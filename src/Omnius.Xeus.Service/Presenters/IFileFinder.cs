using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Presenters
{
    public interface IFileFinderFactory
    {
        ValueTask<IFileFinder> CreateAsync(FileFinderOptions options);
    }

    public class FileFinderOptions
    {
        public FileFinderOptions(string configDirectoryPath, IUserProfileFinder userProfileFinder, IBytesPool bytesPool)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
            this.UserProfileFinder = userProfileFinder;
            this.BytesPool = bytesPool;
        }

        public string ConfigDirectoryPath { get; }

        public IUserProfileFinder UserProfileFinder { get; }

        public IBytesPool BytesPool { get; }
    }

    public interface IFileFinder : IAsyncDisposable
    {
        ValueTask<IEnumerable<XeusFileFoundResult>> FindAll(CancellationToken cancellationToken = default);
    }
}
