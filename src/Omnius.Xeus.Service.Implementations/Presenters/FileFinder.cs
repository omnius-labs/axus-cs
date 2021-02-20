using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Presenters
{
    public sealed class FileFinder : AsyncDisposableBase, IFileFinder
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly FileFinderOptions _options;

        internal sealed class FileFinderFactory : IFileFinderFactory
        {
            public async ValueTask<IFileFinder> CreateAsync(FileFinderOptions options)
            {
                var result = new FileFinder(options);
                await result.InitAsync();

                return result;
            }
        }

        public static IFileFinderFactory Factory { get; } = new FileFinderFactory();

        public FileFinder(FileFinderOptions options)
        {
            _options = options;
        }

        public async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask<IEnumerable<XeusFileFoundResult>> FindAll(CancellationToken cancellationToken = default)
        {
            var profiles = await _options.UserProfileFinder.GetUserProfilesAsync(cancellationToken);
            return profiles.SelectMany(n => n.Content.FileMetas).Select(n => new XeusFileFoundResult(XeusFileFoundResultState.Found, n)).ToList();
        }
    }
}
