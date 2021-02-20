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
    public sealed partial class DeclaredMessagePublisher : AsyncDisposableBase, IDeclaredMessagePublisher
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly DeclaredMessagePublisherOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly DeclaredMessagePublisherRepository _publisherRepo;
        private readonly BlockStogage _blockStorage;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        internal sealed class DeclaredMessagePublisherFactory : IDeclaredMessagePublisherFactory
        {
            public async ValueTask<IDeclaredMessagePublisher> CreateAsync(DeclaredMessagePublisherOptions options, IBytesPool bytesPool)
            {
                var result = new DeclaredMessagePublisher(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IDeclaredMessagePublisherFactory Factory { get; } = new DeclaredMessagePublisherFactory();

        private DeclaredMessagePublisher(DeclaredMessagePublisherOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _publisherRepo = new DeclaredMessagePublisherRepository(Path.Combine(_options.ConfigDirectoryPath, "declared_message_publisher.db"));
            _blockStorage = new BlockStogage(Path.Combine(_options.ConfigDirectoryPath, "blocks.db"), _bytesPool);
        }

        internal async ValueTask InitAsync()
        {
            await _publisherRepo.MigrateAsync();
            await _blockStorage.MigrateAsync();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _publisherRepo.Dispose();
            _blockStorage.Dispose();
        }

        public async ValueTask<DeclaredMessagePublisherReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var itemReports = new List<DeclaredMessagePublishedItemReport>();

                foreach (var item in _publisherRepo.Items.FindAll())
                {
                    itemReports.Add(new DeclaredMessagePublishedItemReport(item.Signature, item.Registrant));
                }

                return new DeclaredMessagePublisherReport(itemReports.ToArray());
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

        public async ValueTask<bool> ContainsMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
                if (item == null) return false;

                return true;
            }
        }

        public async ValueTask PublishMessageAsync(DeclaredMessage message, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var signature = message.Certificate?.GetOmniSignature();
                if (signature is null) throw new ArgumentNullException(nameof(message.Certificate));

                using var hub = new BytesHub(_bytesPool);
                message.Export(hub.Writer, _bytesPool);

                _publisherRepo.Items.Insert(new PublishedDeclaredMessageItem(signature, message.CreationTime.ToDateTime(), registrant));

                var blockName = ComputeBlockName(signature);
                await _blockStorage.WriteAsync(blockName, hub.Reader.GetSequence(), cancellationToken);
            }
        }

        public async ValueTask UnpublishMessageAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _publisherRepo.Items.Delete(signature, registrant);
            }
        }

        public async ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
                if (item == null) return null;

                return item.CreationTime;
            }
        }

        public async ValueTask<DeclaredMessage?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
                if (item == null) return null;

                var blockName = ComputeBlockName(signature);
                var memoryOwner = await _blockStorage.ReadAsync(blockName, cancellationToken);
                if (memoryOwner is null) return null;

                var message = DeclaredMessage.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
                return message;
            }
        }

        private static string ComputeBlockName(OmniSignature signature)
        {
            return StringConverter.SignatureToString(signature);
        }
    }
}
