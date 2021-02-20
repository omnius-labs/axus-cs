using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Presenters
{
    public sealed class UserProfileFinder : AsyncDisposableBase, IUserProfileFinder
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly UserProfileFinderOptions _options;

        private readonly HashSet<OmniSignature> _trustedSignatures = new();
        private readonly Dictionary<OmniSignature, XeusUserProfile> _cachedProfiles = new();

        private readonly Task _watchTask = null!;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const int MaxProfileCount = 32 * 1024;

        internal sealed class UserProfileFinderFactory : IUserProfileFinderFactory
        {
            public async ValueTask<IUserProfileFinder> CreateAsync(UserProfileFinderOptions options)
            {
                var result = new UserProfileFinder(options);
                await result.InitAsync();

                return result;
            }
        }

        public static IUserProfileFinderFactory Factory { get; } = new UserProfileFinderFactory();

        public UserProfileFinder(UserProfileFinderOptions options)
        {
            _options = options;

            _watchTask = this.WatchAsync();
        }

        public async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _watchTask;
            _cancellationTokenSource.Dispose();
        }

        private async Task WatchAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(1000 * 30, cancellationToken);

                await this.UpdateProfilesAsync(cancellationToken);
            }
        }

        private async ValueTask UpdateProfilesAsync(CancellationToken cancellationToken = default)
        {
            var profiles = new HashSet<XeusUserProfile>();

            foreach (var trustedSignature in _options.TrustedSignatures)
            {
                profiles.UnionWith(await this.InternalFindProfilesAsync(trustedSignature, _options.BlockedSignatures, _options.SearchDepth, cancellationToken));
            }

            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _cachedProfiles.Clear();

                foreach (var profile in profiles)
                {
                    _cachedProfiles.Add(profile.Signature, profile);
                }

                _trustedSignatures.Clear();
                _trustedSignatures.UnionWith(_cachedProfiles.Keys);
            }
        }

        private async Task<IEnumerable<XeusUserProfile>> InternalFindProfilesAsync(OmniSignature rootSignature, IEnumerable<OmniSignature> ignoreSignatures, int depth, CancellationToken cancellationToken = default)
        {
            var results = new List<XeusUserProfile>();

            var targetSignatures = new HashSet<OmniSignature>();
            var checkedSignatures = new HashSet<OmniSignature>();
            checkedSignatures.UnionWith(ignoreSignatures);

            targetSignatures.Add(rootSignature);

            foreach (int rank in Enumerable.Range(0, depth))
            {
                var sectionResults = (await this.InternalGetProfilesAsync(targetSignatures, cancellationToken)).ToList();
                if (sectionResults.Count == 0) break;

                checkedSignatures.UnionWith(targetSignatures);
                checkedSignatures.UnionWith(sectionResults.SelectMany(n => n.Content.BlockedSignatures));

                targetSignatures.Clear();
                targetSignatures.UnionWith(sectionResults.SelectMany(n => n.Content.TrustedSignatures).Where(n => !checkedSignatures.Contains(n)));

                results.AddRange(sectionResults);

                if (results.Count > MaxProfileCount) break;
            }

            return results.GetRange(0, MaxProfileCount);
        }

        private async ValueTask<IEnumerable<XeusUserProfile>> InternalGetProfilesAsync(IEnumerable<OmniSignature> signatures, CancellationToken cancellationToken = default)
        {
            var results = new List<XeusUserProfile>();

            foreach (var signature in signatures)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await _options.UserProfileSubscriber.GetUserProfileAsync(signature, cancellationToken);
                if (result is null) continue;

                results.Add(result);
            }

            return results;
        }

        public async ValueTask<XeusUserProfile?> GetUserProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                if (_cachedProfiles.TryGetValue(signature, out var profile))
                {
                    return profile;
                }

                return null;
            }
        }

        public async ValueTask<IEnumerable<XeusUserProfile>> GetUserProfilesAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                return _cachedProfiles.Values;
            }
        }
    }
}
