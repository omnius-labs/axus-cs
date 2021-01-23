// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Nito.AsyncEx;
// using Omnius.Core;
// using Omnius.Core.Cryptography;
// using Omnius.Core.Extensions;
// using Omnius.Xeus.Service.Models;

// namespace Omnius.Xeus.Service.Presenters
// {
//     public sealed class FileFinder : AsyncDisposableBase, IFileFinder
//     {
//         private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

//         private string _configDirectoryPath;
//         private IUserProfileFinder _userProfileFinder;
//         private IBytesPool _bytesPool;

//         private FileFinderConfig _config;

//         private readonly Task _watchTask = null!;
//         private readonly CancellationTokenSource _cancellationTokenSource = new();

//         private readonly AutoResetEvent _resetEvent = new(true);
//         private readonly AsyncReaderWriterLock _asyncLock = new();
//         private readonly object _lockObject = new();

//         private const int MaxProfileCount = 32 * 1024;

//         internal sealed class FileFinderFactory : IFileFinderFactory
//         {
//             public async ValueTask<IFileFinder> CreateAsync(FileFinderOptions options)
//             {
//                 var result = new FileFinder(options);
//                 await result.InitAsync();

//                 return result;
//             }
//         }

//         public FileFinder(FileFinderOptions options)
//         {
//             _watchTask = this.WatchAsync();
//             _bytesPool = options.BytesPool ?? BytesPool.Shared;
//         }

//         public async ValueTask InitAsync()
//         {
//         }

//         protected override async ValueTask OnDisposeAsync()
//         {
//             _cancellationTokenSource.Cancel();
//             await _watchTask;
//             _cancellationTokenSource.Dispose();
//         }

//         private async Task WatchAsync(CancellationToken cancellationToken = default)
//         {
//             await Task.Delay(1, cancellationToken).ConfigureAwait(false);

//             for (; ; )
//             {
//                 await _resetEvent.WaitAsync(1000 * 30, cancellationToken);

//                 await this.UpdateProfilesAsync(cancellationToken);
//             }
//         }

//         private async ValueTask UpdateProfilesAsync(CancellationToken cancellationToken = default)
//         {
//             var profiles = new HashSet<XeusUserProfile>();

//             var config = _config;

//             foreach (var trustedSignature in config.TrustedSignatures)
//             {
//                 profiles.UnionWith(await this.InternalFindProfilesAsync(trustedSignature, config.BlockedSignatures, config.SearchDepth, cancellationToken));
//             }

//             using (await _asyncLock.WriterLockAsync(cancellationToken))
//             {
//                 if (_config != config)
//                 {
//                     return;
//                 }

//                 _cachedProfiles.Clear();

//                 foreach (var profile in profiles)
//                 {
//                     _cachedProfiles.Add(profile.Signature, profile);
//                 }

//                 _trustedSignatures.Clear();
//                 _trustedSignatures.UnionWith(_cachedProfiles.Keys);
//             }
//         }

//         private async Task<IEnumerable<XeusUserProfile>> InternalFindProfilesAsync(OmniSignature rootSignature, IEnumerable<OmniSignature> ignoreSignatures, int depth, CancellationToken cancellationToken = default)
//         {
//             var results = new List<XeusUserProfile>();

//             var targetSignatures = new HashSet<OmniSignature>();
//             var checkedSignatures = new HashSet<OmniSignature>();
//             checkedSignatures.UnionWith(ignoreSignatures);

//             targetSignatures.Add(rootSignature);

//             foreach (int rank in Enumerable.Range(0, depth))
//             {
//                 var sectionResults = (await this.InternalGetProfilesAsync(targetSignatures, cancellationToken)).ToList();
//                 if (sectionResults.Count == 0)
//                 {
//                     break;
//                 }

//                 checkedSignatures.UnionWith(targetSignatures);
//                 checkedSignatures.UnionWith(sectionResults.SelectMany(n => n.Content.BlockedSignatures));

//                 targetSignatures.Clear();
//                 targetSignatures.UnionWith(sectionResults.SelectMany(n => n.Content.TrustedSignatures).Where(n => !checkedSignatures.Contains(n)));

//                 results.AddRange(sectionResults);

//                 if (results.Count > MaxProfileCount)
//                 {
//                     break;
//                 }
//             }

//             return results.GetRange(0, MaxProfileCount);
//         }

//         private async ValueTask<IEnumerable<XeusUserProfile>> InternalGetProfilesAsync(IEnumerable<OmniSignature> signatures, CancellationToken cancellationToken = default)
//         {
//             var results = new List<XeusUserProfile>();

//             foreach (var signature in signatures)
//             {
//                 cancellationToken.ThrowIfCancellationRequested();

//                 var result = await _userProfileSubscriber.GetUserProfileAsync(signature, cancellationToken);
//                 if (result is null)
//                 {
//                     continue;
//                 }

//                 results.Add(result);
//             }

//             return results;
//         }

//         public FileFinderConfig Config => _config;

//         public async ValueTask SetConfigAsync(FileFinderConfig config, CancellationToken cancellationToken = default)
//         {
//             using (await _asyncLock.WriterLockAsync(cancellationToken))
//             {
//                 _config = config;
//                 _resetEvent.Set();
//             }
//         }

//         public async ValueTask<XeusUserProfile?> GetUserProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default)
//         {
//             using (await _asyncLock.ReaderLockAsync(cancellationToken))
//             {
//                 if (_cachedProfiles.TryGetValue(signature, out var profile))
//                 {
//                     return profile;
//                 }

//                 return null;
//             }
//         }

//         public async ValueTask<IEnumerable<XeusUserProfile>> GetUserProfilesAsync(CancellationToken cancellationToken = default)
//         {
//             using (await _asyncLock.ReaderLockAsync(cancellationToken))
//             {
//                 return _cachedProfiles.Values;
//             }
//         }

//         public ValueTask<IEnumerable<XeusFileMeta>> FindAll(CancellationToken cancellationToken = default)
//         {
//             throw new System.NotImplementedException();
//         }
//     }
// }
