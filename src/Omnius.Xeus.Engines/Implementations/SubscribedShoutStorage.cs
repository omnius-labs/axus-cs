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
using Omnius.Xeus.Engines.Internal;
using Omnius.Xeus.Engines.Internal.Models;

namespace Omnius.Xeus.Engines
{
    public sealed partial class SubscribedShoutStorage : AsyncDisposableBase, ISubscribedShoutStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SubscribedShoutStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly SubscribedShoutStorageRepository _subscriberRepo;
        private readonly IBytesStorage<string> _blockStorage;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        internal sealed class SubscribedShoutStorageFactory : ISubscribedShoutStorageFactory
        {
            public async ValueTask<ISubscribedShoutStorage> CreateAsync(SubscribedShoutStorageOptions options, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default)
            {
                var result = new SubscribedShoutStorage(options, bytesStorageFactory, bytesPool);
                await result.InitAsync(cancellationToken);

                return result;
            }
        }

        public static ISubscribedShoutStorageFactory Factory { get; } = new SubscribedShoutStorageFactory();

        private SubscribedShoutStorage(SubscribedShoutStorageOptions options, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _subscriberRepo = new SubscribedShoutStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _blockStorage = bytesStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
        }

        internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
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
                var itemReports = new List<SubscribedShoutStorageReport>();

                foreach (var item in _subscriberRepo.Items.FindAll())
                {
                    itemReports.Add(new SubscribedShoutStorageReport(item.Signature, item.Registrant));
                }

                return new SubscribedShoutStorageReport(itemReports.ToArray());
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

        public async ValueTask SubscribeMessageAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Insert(new SubscribedShoutItem(signature, registrant));
            }
        }

        public async ValueTask UnsubscribeMessageAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
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

            using var hub = new BytesPipe(_bytesPool);
            message.Export(hub.Writer, _bytesPool);

            _subscriberRepo.WrittenItems.Insert(new WrittenDeclaredMessageItem(signature, message.CreationTime.ToDateTime()));

            var blockName = ComputeBlockName(signature);
            await _blockStorage.WriteAsync(blockName, hub.Reader.GetSequence(), cancellationToken);
        }

        private static string ComputeBlockName(OmniSignature signature)
        {
            return StringConverter.SignatureToString(signature);
        }
    }
}
