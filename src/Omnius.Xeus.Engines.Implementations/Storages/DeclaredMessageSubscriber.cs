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
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Internal;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories;

namespace Omnius.Xeus.Engines.Storages
{
    public sealed partial class DeclaredMessageSubscriber : AsyncDisposableBase, IDeclaredMessageSubscriber
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly DeclaredMessageSubscriberOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly DeclaredMessageSubscriberRepository _subscriberRepo;
        private readonly BlockStogage _blockStorage;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        internal sealed class DeclaredMessageSubscriberFactory : IDeclaredMessageSubscriberFactory
        {
            public async ValueTask<IDeclaredMessageSubscriber> CreateAsync(DeclaredMessageSubscriberOptions options, IBytesPool bytesPool)
            {
                var result = new DeclaredMessageSubscriber(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IDeclaredMessageSubscriberFactory Factory { get; } = new DeclaredMessageSubscriberFactory();

        private DeclaredMessageSubscriber(DeclaredMessageSubscriberOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _subscriberRepo = new DeclaredMessageSubscriberRepository(Path.Combine(_options.ConfigDirectoryPath, "declared_message_subscriber.db"));
            _blockStorage = new BlockStogage(Path.Combine(_options.ConfigDirectoryPath, "blocks.db"), _bytesPool);
        }

        internal async ValueTask InitAsync()
        {
            await _subscriberRepo.MigrateAsync();
            await _blockStorage.MigrateAsync();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _subscriberRepo.Dispose();
            _blockStorage.Dispose();
        }

        public async ValueTask<DeclaredMessageSubscriberReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var itemReports = new List<DeclaredMessageSubscribedItemReport>();

                foreach (var item in _subscriberRepo.Items.FindAll())
                {
                    itemReports.Add(new DeclaredMessageSubscribedItemReport(item.Signature, item.Registrant));
                }

                return new DeclaredMessageSubscriberReport(itemReports.ToArray());
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

        public async ValueTask<bool> ContainsMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
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
                _subscriberRepo.Items.Insert(new SubscribedDeclaredMessageItem(signature, registrant));
            }
        }

        public async ValueTask UnsubscribeMessageAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Delete(signature, registrant);
            }
        }

        public async ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
                if (writtenItem == null) return null;

                return writtenItem.CreationTime;
            }
        }

        public async ValueTask<DeclaredMessage?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var item = _subscriberRepo.Items.Find(signature).FirstOrDefault();
            if (item is null) return null;

            var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
            if (writtenItem is null) return null;

            var blockName = ComputeBlockName(signature);
            using var memoryOwner = await _blockStorage.ReadAsync(blockName, cancellationToken);
            if (memoryOwner is null) return null;

            var message = DeclaredMessage.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
            return message;
        }

        public async ValueTask WriteMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default)
        {
            if (!message.Verify()) return;

            var signature = message.Certificate?.GetOmniSignature();
            if (signature == null) return;

            if (!_subscriberRepo.Items.Exists(signature)) return;

            var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
            if (writtenItem is not null && writtenItem.CreationTime >= message.CreationTime.ToDateTime()) return;

            using var hub = new BytesHub(_bytesPool);
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
