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
using Omnius.Core.Storages;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Internal;
using Omnius.Xeus.Engines.Internal.Models;
using Omnius.Xeus.Engines.Internal.Repositories;

namespace Omnius.Xeus.Engines
{
    public sealed partial class PublishedShoutStorage : AsyncDisposableBase, IPublishedShoutStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PublishedShoutStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly PublishedShoutStorageRepository _publisherRepo;
        private readonly IBytesStorage<string> _blockStorage;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        internal sealed class PublishedShoutStorageFactory : IPublishedShoutStorageFactory
        {
            public async ValueTask<IPublishedShoutStorage> CreateAsync(PublishedShoutStorageOptions options, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default)
            {
                var result = new PublishedShoutStorage(options, bytesStorageFactory, bytesPool);
                await result.InitAsync(cancellationToken);

                return result;
            }
        }

        public static IPublishedShoutStorageFactory Factory { get; } = new PublishedShoutStorageFactory();

        private PublishedShoutStorage(PublishedShoutStorageOptions options, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _publisherRepo = new PublishedShoutStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _blockStorage = bytesStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
        }

        internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            await _publisherRepo.MigrateAsync(cancellationToken);
            await _blockStorage.MigrateAsync(cancellationToken);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _publisherRepo.Dispose();
            _blockStorage.Dispose();
        }

        public async ValueTask<PublishedShoutStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var itemReports = new List<PublishedShoutStorageReport>();

                foreach (var item in _publisherRepo.Items.FindAll())
                {
                    itemReports.Add(new PublishedShoutStorageReport(item.Signature, item.Registrant));
                }

                return new PublishedShoutStorageReport(itemReports.ToArray());
            }
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var results = new List<OmniSignature>();

                foreach (var status in _publisherRepo.Items.FindAll())
                {
                    results.Add(status.Signature);
                }

                return results;
            }
        }

        public async ValueTask<bool> ContainsShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
                if (item == null) return false;

                return true;
            }
        }

        public async ValueTask PublishShoutAsync(Shout message, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var signature = message.Certificate?.GetOmniSignature();
                if (signature is null) throw new ArgumentNullException(nameof(message.Certificate));

                using var hub = new BytesPipe(_bytesPool);
                message.Export(hub.Writer, _bytesPool);

                _publisherRepo.Items.Insert(new PublishedShoutStorageItem(signature, message.CreationTime.ToDateTime(), registrant));

                var blockName = ComputeBlockName(signature);
                await _blockStorage.WriteAsync(blockName, hub.Reader.GetSequence(), cancellationToken);
            }
        }

        public async ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _publisherRepo.Items.Delete(signature, registrant);
            }
        }

        public async ValueTask<DateTime?> ReadShoutCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
                if (item == null) return null;

                return item.CreationTime;
            }
        }

        public async ValueTask<Shout?> ReadShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
                if (item == null) return null;

                var blockName = ComputeBlockName(signature);
                var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
                if (memoryOwner is null) return null;

                var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
                return message;
            }
        }

        private static string ComputeBlockName(OmniSignature signature)
        {
            return StringConverter.SignatureToString(signature);
        }
    }
}
