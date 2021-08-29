using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Internal.Repositories;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Intaractors
{
    public record UploadingUserProfileReport
    {
        public UploadingUserProfileReport(DateTime creationTime, OmniSignature signature)
        {
            this.CreationTime = creationTime;
            this.Signature = signature;
        }

        public DateTime CreationTime { get; }

        public OmniSignature Signature { get; }
    }

    public sealed class UserProfileUploader : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IXeusApi _xeusApi;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly UserProfileUploaderOptions _options;

        private readonly UserProfileUploaderRepository _userProfileUploaderRepo;

        private readonly Task _watchLoopTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const string Registrant = "Omnius.Xeus.Intaractors.UserProfileUploader";

        public UserProfileUploader(IXeusApi xeusApi, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, UserProfileUploaderOptions options)
        {
            _xeusApi = xeusApi;
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _userProfileUploaderRepo = new UserProfileUploaderRepository(Path.Combine(options.ConfigDirectoryPath, "state"));
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

                    await this.SyncPublishedDeclaredMessage(cancellationToken);
                    await this.SyncPublishedFileStorage(cancellationToken);
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

        private async Task SyncPublishedDeclaredMessage(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedItems = await this.InternalGetPublishedDeclaredMessageReportsAsync(cancellationToken);
                var set = new HashSet<OmniSignature>();
                set.UnionWith(publishedItems.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

                foreach (var signature in set)
                {
                    if (_userProfileUploaderRepo.Items.Exists(signature)) continue;
                    await this.InternalUnpublishDeclaredMessageAsync(signature, cancellationToken);
                }

                foreach (var signature in _userProfileUploaderRepo.Items.FindAll().Select(n => n.Signature))
                {
                    if (set.Contains(signature)) continue;
                    _userProfileUploaderRepo.Items.Delete(signature);
                }
            }
        }

        private async Task SyncPublishedFileStorage(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedItems = await this.InternalGetPublishedContentReportsAsync(cancellationToken);
                var set = new HashSet<OmniHash>();
                set.UnionWith(publishedItems.Where(n => n.Registrant == Registrant).Select(n => n.RootHash).Where(n => n.HasValue).Select(n => n!.Value));

                foreach (var rootHash in set)
                {
                    if (_userProfileUploaderRepo.Items.Exists(rootHash)) continue;
                    await this.InternalUnpublishContentAsync(rootHash, cancellationToken);
                }

                foreach (var rootHash in _userProfileUploaderRepo.Items.FindAll().Select(n => n.RootHash))
                {
                    if (set.Contains(rootHash)) continue;
                    _userProfileUploaderRepo.Items.Delete(rootHash);
                }
            }
        }

        public async ValueTask<IEnumerable<UploadingUserProfileReport>> GetUploadingUserProfileReportsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var reports = new List<UploadingUserProfileReport>();

                foreach (var item in _userProfileUploaderRepo.Items.FindAll())
                {
                    reports.Add(new UploadingUserProfileReport(item.CreationTime, item.Signature));
                }

                return reports;
            }
        }

        public async ValueTask RegisterAsync(UserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var rootHash = await this.PublishContentAsync(content, cancellationToken);
                await this.InternalPublishDeclaredMessageAsync(rootHash, digitalSignature, cancellationToken);

                var item = new UploadingUserProfileItem(digitalSignature.GetOmniSignature(), rootHash, Timestamp.FromDateTime(DateTime.UtcNow));
                _userProfileUploaderRepo.Items.Upsert(item);
            }
        }

        public async ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var item = _userProfileUploaderRepo.Items.FindOne(signature);
                if (item is null) return;

                await this.InternalUnpublishDeclaredMessageAsync(item.Signature, cancellationToken);
                await this.InternalUnpublishContentAsync(item.RootHash, cancellationToken);

                _userProfileUploaderRepo.Items.Delete(item.Signature);
            }
        }

        private async ValueTask<OmniHash> PublishContentAsync(UserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using var contentBytes = RocketMessage.ToBytes(content);
            var rootHash = await _xeusApi.PublishFileFromMemoryAsync(contentBytes.Memory, Registrant, cancellationToken);

            using var rootHashBytes = RocketMessage.ToBytes(rootHash);
            using var shout = Shout.Create(Timestamp.FromDateTime(DateTime.UtcNow), rootHashBytes, digitalSignature);
            await _xeusApi.PublishShoutAsync(shout, Registrant, cancellationToken);
        }

        private async ValueTask InternalPublishDeclaredMessageAsync(OmniHash rootHash, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesPipe();
            rootHash.Export(hub.Writer, _bytesPool);

            var sequence = hub.Reader.GetSequence();
            var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            sequence.CopyTo(memoryOwner.Memory.Span);

            using var declaredMessage = Shout.Create(Timestamp.FromDateTime(DateTime.UtcNow), memoryOwner, digitalSignature);
            var input = new PublishedDeclaredMessage_PublishMessage_Input(declaredMessage, Registrant);
            await _xeusApi.PublishedDeclaredMessage_PublishMessageAsync(input, cancellationToken);
        }

        private async ValueTask InternalUnpublishDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new PublishedDeclaredMessage_UnpublishMessage_Input(signature, Registrant);
            await _xeusApi.PublishedDeclaredMessage_UnpublishMessageAsync(input, cancellationToken);
        }

        private async ValueTask InternalUnpublishContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            var input = new PublishedFileStorage_UnpublishContent_Memory_Input(rootHash, Registrant);
            await _xeusApi.PublishedFileStorage_UnpublishContentAsync(input, cancellationToken);
        }
    }
}
