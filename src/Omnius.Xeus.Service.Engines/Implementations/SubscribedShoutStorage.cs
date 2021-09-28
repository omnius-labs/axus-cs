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
using Omnius.Core.Storages;
using Omnius.Xeus.Service.Engines.Internal;
using Omnius.Xeus.Service.Engines.Internal.Models;
using Omnius.Xeus.Service.Engines.Internal.Repositories;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class SubscribedShoutStorage : AsyncDisposableBase, ISubscribedShoutStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly SubscribedShoutStorageOptions _options;

        private readonly SubscribedShoutStorageRepository _subscriberRepo;
        private readonly IBytesStorage<string> _blockStorage;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        public static async ValueTask<SubscribedShoutStorage> CreateAsync(IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, SubscribedShoutStorageOptions options, CancellationToken cancellationToken = default)
        {
            var subscribedShoutStorage = new SubscribedShoutStorage(bytesStorageFactory, bytesPool, options);
            await subscribedShoutStorage.InitAsync(cancellationToken);
            return subscribedShoutStorage;
        }

        private SubscribedShoutStorage(IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, SubscribedShoutStorageOptions options)
        {
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _subscriberRepo = new SubscribedShoutStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _blockStorage = _bytesStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
        }

        private async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            await _subscriberRepo.MigrateAsync(cancellationToken);
            await _blockStorage.MigrateAsync(cancellationToken);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _subscriberRepo.Dispose();
            _blockStorage.Dispose();
        }

        public async ValueTask<SubscribedShoutStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var shoutReports = new List<SubscribedShoutReport>();

                foreach (var item in _subscriberRepo.Items.FindAll())
                {
                    shoutReports.Add(new SubscribedShoutReport(item.Signature, item.Registrant));
                }

                return new SubscribedShoutStorageReport(shoutReports.ToArray());
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

                foreach (var item in _subscriberRepo.Items.FindAll())
                {
                    results.Add(item.Signature);
                }

                return results;
            }
        }

        public async ValueTask<bool> ContainsShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                if (_subscriberRepo.Items.Exists(signature)) return false;

                return true;
            }
        }

        public async ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Insert(new SubscribedShoutItem(signature, registrant));
            }
        }

        public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Delete(signature, registrant);
            }
        }

        public async ValueTask<DateTime?> ReadShoutCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
                if (writtenItem == null) return null;

                return writtenItem.CreationTime;
            }
        }

        public async ValueTask<Shout?> ReadShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var item = _subscriberRepo.Items.Find(signature).FirstOrDefault();
            if (item is null) return null;

            var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
            if (writtenItem is null) return null;

            var blockName = ComputeBlockName(signature);
            using var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
            if (memoryOwner is null) return null;

            var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
            return message;
        }

        public async ValueTask WriteShoutAsync(Shout message, CancellationToken cancellationToken = default)
        {
            if (!message.Verify()) return;

            var signature = message.Certificate?.GetOmniSignature();
            if (signature == null) return;

            if (!_subscriberRepo.Items.Exists(signature)) return;

            var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
            if (writtenItem is not null && writtenItem.CreationTime >= message.CreationTime.ToDateTime()) return;

            using var bytesPise = new BytesPipe(_bytesPool);
            message.Export(bytesPise.Writer, _bytesPool);

            _subscriberRepo.WrittenItems.Insert(new WrittenShoutItem(signature, message.CreationTime.ToDateTime()));

            var blockName = ComputeBlockName(signature);
            await _blockStorage.WriteAsync(blockName, bytesPise.Reader.GetSequence(), cancellationToken);
        }

        private static string ComputeBlockName(OmniSignature signature)
        {
            return StringConverter.SignatureToString(signature);
        }
    }
}
