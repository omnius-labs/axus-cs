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
using Omnius.Xeus.Intaractors.Internal;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Service.Remoting;

namespace Omnius.Xeus.Intaractors
{
    public sealed partial class ProfileSubscriber : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly XeusServiceAdapter _service;
        private readonly IKeyValueStorageFactory _keyValueStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly ProfileSubscriberOptions _options;

        private ProfileDownloader _profileDownloader = null!;
        private readonly ISingleValueStorage _configStorage;

        private readonly IKeyValueStorage<string> _cachedProfileStorage;

        private Task _watchLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const string Registrant = "Omnius.Xeus.Intaractors.ProfileSubscriber";

        public static async ValueTask<ProfileSubscriber> CreateAsync(IXeusService xeusService, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfileSubscriberOptions options, CancellationToken cancellationToken = default)
        {
            var profileSubscriber = new ProfileSubscriber(xeusService, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
            await profileSubscriber.InitAsync(cancellationToken);
            return profileSubscriber;
        }

        private ProfileSubscriber(IXeusService xeusService, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfileSubscriberOptions options)
        {
            _service = new XeusServiceAdapter(xeusService);
            _keyValueStorageFactory = keyValueStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
            _cachedProfileStorage = keyValueStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "profiles"), _bytesPool);
        }

        internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            await _cachedProfileStorage.MigrateAsync(cancellationToken);

            _profileDownloader = await ProfileDownloader.CreateAsync(_service, _keyValueStorageFactory, _bytesPool, Path.Combine(_options.ConfigDirectoryPath, "downloader"), cancellationToken);

            _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _watchLoopTask;
            _cancellationTokenSource.Dispose();
        }

        private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
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
                var config = await this.GetConfigAsync(cancellationToken);

                var allTargetSignatures = new HashSet<OmniSignature>();

                await foreach (var profile in this.InternalRecursiveFindProfilesAsync(config.TrustedSignatures, config.BlockedSignatures, (int)config.SearchDepth, (int)config.MaxProfileCount, cancellationToken))
                {
                    allTargetSignatures.Add(profile.Signature);
                }

                var removedKeys = new HashSet<string>();
                await foreach (var key in _cachedProfileStorage.GetKeysAsync(cancellationToken))
                {
                    if (allTargetSignatures.Contains(key)) continue;

                    removedKeys.Add(key);
                }

                foreach (var key in removedKeys)
                {
                    await _cachedProfileStorage.TryDeleteAsync(key, cancellationToken);
                }
            }
        }

        private async IAsyncEnumerable<Profile> InternalRecursiveFindProfilesAsync(IEnumerable<OmniSignature> rootSignatures, IEnumerable<OmniSignature> ignoreSignatures, int depth, int maxCount, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

        private async IAsyncEnumerable<Profile> InternalFindProfilesAsync(IEnumerable<OmniSignature> signatures, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var signature in signatures)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cachedProfile = await _cachedProfileStorage.TryGetValueAsync<Profile>(StringConverter.SignatureToString(signature), cancellationToken);
                var downloadedProfile = await _profileDownloader.ExportAsync(signature, cancellationToken);

                if (cachedProfile is not null)
                {
                    if (downloadedProfile is null)
                    {
                        yield return cachedProfile;
                    }
                    else
                    {
                        yield return cachedProfile.CreationTime <= downloadedProfile.CreationTime ? downloadedProfile : cachedProfile;
                    }
                }
                else if (downloadedProfile is not null)
                {
                    yield return downloadedProfile;
                }
            }
        }

        public async ValueTask<Profile?> FindOneAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                return await _cachedProfileStorage.TryGetValueAsync<Profile>(StringConverter.SignatureToString(signature), cancellationToken);
            }
        }

        public async IAsyncEnumerable<Profile> FindAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                await foreach (var value in _cachedProfileStorage.GetValuesAsync<Profile>(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return value;
                }
            }
        }

        public async ValueTask<ProfileSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var config = await _configStorage.TryGetValueAsync<ProfileSubscriberConfig>(cancellationToken);
                if (config is null) return ProfileSubscriberConfig.Empty;

                return config;
            }
        }

        public async ValueTask SetConfigAsync(ProfileSubscriberConfig config, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }
        }
    }
}
