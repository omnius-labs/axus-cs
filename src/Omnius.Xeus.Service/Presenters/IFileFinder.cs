using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
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
        FileFinderConfig Config { get; }

        ValueTask SetConfigAsync(FileFinderConfig config, CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<XeusFileMeta>> FindAll(CancellationToken cancellationToken = default);
    }

    public class FileFinderConfig
    {
        public FileFinderConfig()
        {
        }
    }
}
