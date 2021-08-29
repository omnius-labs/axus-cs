using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Storages;
using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors
{
    public sealed class UserProfileFinder : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly UserProfileFinderOptions _options;
        private readonly IUserProfileDownloader _userProfileDownloader;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;

        private readonly IBytesStorage<string> _optionsStorage;
        private readonly IBytesStorage<OmniSignatureEntity> _cachedUserProfileStorage;

        private Task _watchTask = null!;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        internal sealed class UserProfileFinderFactory : IUserProfileFinderFactory
        {
            public async ValueTask<IUserProfileFinder> CreateAsync(UserProfileFinderOptions options, IUserProfileDownloader userProfileDownloader,
                IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default)
            {
                var result = new UserProfileFinder(options, userProfileDownloader, bytesStorageFactory, bytesPool);
                await result.InitAsync(cancellationToken);

                return result;
            }
        }

        public static IUserProfileFinderFactory Factory { get; } = new UserProfileFinderFactory();

        public UserProfileFinder(UserProfileFinderOptions options, IUserProfileDownloader userProfileDownloader,
            IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool)
        {
            _options = options;
            _userProfileDownloader = userProfileDownloader;
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;

            _optionsStorage = _bytesStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "options"), _bytesPool);
            _cachedUserProfileStorage = _bytesStorageFactory.Create<OmniSignatureEntity>(Path.Combine(_options.ConfigDirectoryPath, "user_profiles"), _bytesPool);
        }

        internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            _watchTask = this.WatchAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await _watchTask;

            _cancellationTokenSource.Dispose();
        }

        private async Task WatchAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);

                for (; ; )
                {
                    await Task.Delay(1000 * 30, cancellationToken);

                    await this.UpdateProfilesAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private async ValueTask UpdateProfilesAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var searchOptions = await _optionsStorage.TryGetValueAsync<string, UserProfileFinderSearchOptions>("search_options", cancellationToken);
                searchOptions ??= UserProfileFinderSearchOptions.Empty;

                var allTargetSignatures = new HashSet<OmniSignature>();

                await foreach (var profile in this.InternalRecursiveFindProfilesAsync(searchOptions.TrustedSignatures, searchOptions.BlockedSignatures, (int)searchOptions.SearchDepth, (int)searchOptions.MaxUserProfileCount, cancellationToken))
                {
                    allTargetSignatures.Add(profile.Signature);
                }

                var removedSignature = new HashSet<OmniSignature>();
                await foreach (var signatureEntity in _cachedUserProfileStorage.GetKeysAsync(cancellationToken))
                {
                    var signature = signatureEntity.Export();
                    if (allTargetSignatures.Contains(signature)) continue;
                    removedSignature.Add(signature);
                }

                foreach (var signature in removedSignature)
                {
                    await _cachedUserProfileStorage.TryDeleteAsync(OmniSignatureEntity.Import(signature), cancellationToken);
                }
            }
        }

        private async IAsyncEnumerable<UserProfile> InternalRecursiveFindProfilesAsync(IEnumerable<OmniSignature> rootSignatures, IEnumerable<OmniSignature> ignoreSignatures, int depth, int maxCount, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (maxCount == 0) yield break;

            var targetSignatures = new HashSet<OmniSignature>();
            var checkedSignatures = new HashSet<OmniSignature>();

            targetSignatures.UnionWith(rootSignatures);
            checkedSignatures.UnionWith(ignoreSignatures);

            int count = 0;

            foreach (int rank in Enumerable.Range(0, depth))
            {
                if (targetSignatures.Count == 0) break;

                var nextTargetSignatures = new HashSet<OmniSignature>();

                await foreach (var profile in this.InternalFindProfilesAsync(targetSignatures, cancellationToken))
                {
                    checkedSignatures.Add(profile.Signature);
                    checkedSignatures.UnionWith(profile.Content.BlockedSignatures);
                    nextTargetSignatures.UnionWith(profile.Content.TrustedSignatures);

                    yield return profile;

                    if (++count >= maxCount) yield break;
                }

                nextTargetSignatures.ExceptWith(checkedSignatures);
                targetSignatures = nextTargetSignatures;
            }
        }

        private async IAsyncEnumerable<UserProfile> InternalFindProfilesAsync(IEnumerable<OmniSignature> signatures, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var signature in signatures)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cachedProfile = await _cachedUserProfileStorage.TryGetValueAsync<OmniSignatureEntity, UserProfile>(OmniSignatureEntity.Import(signature), cancellationToken);
                var downloadedProfile = await _userProfileDownloader.ExportAsync(signature, cancellationToken);

                if (cachedProfile is not null)
                {
                    if (downloadedProfile is not null)
                    {
                        if (cachedProfile.CreationTime <= downloadedProfile.CreationTime)
                        {
                            yield return downloadedProfile;
                        }
                        else
                        {
                            yield return cachedProfile;
                        }
                    }
                    else
                    {
                        yield return cachedProfile;
                    }
                }
                else if (downloadedProfile is not null)
                {
                    yield return downloadedProfile;
                }
            }
        }

        public async ValueTask<UserProfile?> FindOneAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                return await _cachedUserProfileStorage.TryGetValueAsync<OmniSignatureEntity, UserProfile>(OmniSignatureEntity.Import(signature), cancellationToken);
            }
        }

        public async IAsyncEnumerable<UserProfile> FindAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                await foreach (var value in _cachedUserProfileStorage.GetValuesAsync<OmniSignatureEntity, UserProfile>(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return value;
                }
            }
        }

        public async ValueTask<UserProfileFinderSearchOptions> GetSearchOptionsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var result = await _optionsStorage.TryGetValueAsync<string, UserProfileFinderSearchOptions>("search_options", cancellationToken);
                return result ?? UserProfileFinderSearchOptions.Empty;
            }
        }

        public async ValueTask SetSearchOptionsAsync(UserProfileFinderSearchOptions searchOptions, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await _optionsStorage.SetValueAsync("search_options", searchOptions, cancellationToken);
            }
        }
    }
}
