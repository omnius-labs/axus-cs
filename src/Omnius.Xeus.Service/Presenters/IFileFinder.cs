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
        public string? ConfigDirectoryPath { get; init; }

        public IUserProfileFinder? UserProfileFinder { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IFileFinder
    {
        ValueTask<IEnumerable<XeusFileFoundResult>> FindAll(CancellationToken cancellationToken = default);
    }
}
