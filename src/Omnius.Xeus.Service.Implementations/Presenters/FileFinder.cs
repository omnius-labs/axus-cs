using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Presenters
{
    public sealed class FileFinder : AsyncDisposableBase, IFileFinder
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string _configDirectoryPath;
        private IUserProfileFinder _userProfileFinder;
        private IBytesPool _bytesPool;

        private readonly AutoResetEvent _resetEvent = new(true);
        private readonly AsyncReaderWriterLock _asyncLock = new();
        private readonly object _lockObject = new();

        private const int MaxProfileCount = 32 * 1024;

        internal sealed class FileFinderFactory : IFileFinderFactory
        {
            public async ValueTask<IFileFinder> CreateAsync(FileFinderOptions options)
            {
                var result = new FileFinder(options);
                await result.InitAsync();

                return result;
            }
        }

        public FileFinder(FileFinderOptions options)
        {
            _userProfileFinder = options.UserProfileFinder ?? throw new ArgumentNullException(nameof(options.UserProfileFinder));
            _bytesPool = options.BytesPool ?? BytesPool.Shared;
        }

        public async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask<IEnumerable<XeusFileFoundResult>> FindAll(CancellationToken cancellationToken = default)
        {
            var profiles = await _userProfileFinder.GetUserProfilesAsync(cancellationToken);
            return profiles.SelectMany(n => n.Content.FileMetas).Select(n => new XeusFileFoundResult(XeusFileFoundResultState.Found, n)).ToList();
        }
    }
}
