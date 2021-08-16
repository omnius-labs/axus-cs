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
using Omnius.Xeus.Engines.Internal;
using Omnius.Xeus.Engines.Internal.Models;
using Omnius.Xeus.Engines.Internal.Repositories;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public record SubscribedShoutStorageOptions
    {
        public string? ConfigDirectoryPath { get; init; }
        public IBytesStorageFactory? BytesStorageFactory { get; init; }
        public IBytesPool? BytesPool { get; init; }
    }

    public sealed partial class SubscribedShoutStorage : AsyncDisposableBase, ISubscribedShoutStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SubscribedShoutStorageOptions _options;

        private readonly SubscribedShoutStorageRepository _subscriberRepo;
        private readonly IBytesStorage<string> _blockStorage;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private SubscribedShoutStorage(SubscribedShoutStorageOptions options)
        {
            _options = options;

            _subscriberRepo = new SubscribedShoutStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _blockStorage = _options.BytesStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _options.BytesPool);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _subscriberRepo.Dispose();
            _blockStorage.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            await _subscriberRepo.MigrateAsync(cancellationToken);
            await _blockStorage.MigrateAsync(cancellationToken);
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

            var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _options.BytesPool);
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

            using var bytesPise = new BytesPipe(_options.BytesPool);
            message.Export(bytesPise.Writer, _options.BytesPool);

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
