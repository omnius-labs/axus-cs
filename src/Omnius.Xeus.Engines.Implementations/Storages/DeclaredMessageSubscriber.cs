using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories;

namespace Omnius.Xeus.Engines.Storages
{
    public sealed partial class DeclaredMessageSubscriber : AsyncDisposableBase, IDeclaredMessageSubscriber
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly DeclaredMessageSubscriberOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly DeclaredMessageSubscriberRepository _repository;
        private readonly string _cachePath;

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

            _repository = new DeclaredMessageSubscriberRepository(Path.Combine(_options.ConfigDirectoryPath, "want-declared-message-storage.db"));
            _cachePath = Path.Combine(_options.ConfigDirectoryPath, "cache");
        }

        internal async ValueTask InitAsync()
        {
            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }

            await _repository.MigrateAsync();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _repository.Dispose();
        }

        public async ValueTask<DeclaredMessageSubscriberReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<OmniSignature>();

            foreach (var status in _repository.Items.GetAll())
            {
                results.Add(status.Signature);
            }

            return results;
        }

        public async ValueTask<bool> ContainsMessageAsync(OmniSignature signature, DateTime since = default)
        {
            var status = _repository.Items.Get(signature);
            if (status == null)
            {
                return false;
            }

            return true;
        }

        public async ValueTask SubscribeMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            _repository.Items.Add(new DeclaredMessageSubscriberItem(signature, DateTime.MinValue));
        }

        public async ValueTask UnsubscribeMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            _repository.Items.Remove(signature);
        }

        public async ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var status = _repository.Items.Get(signature);
            if (status == null)
            {
                return null;
            }

            return status.CreationTime;
        }

        public async ValueTask<DeclaredMessage?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var status = _repository.Items.Get(signature);
            if (status == null)
            {
                return null;
            }

            var filePath = Path.Combine(_cachePath, StringConverter.SignatureToString(signature));

            using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
            var pipeReader = PipeReader.Create(fileStream);
            var readResult = await pipeReader.ReadAsync(cancellationToken);
            var message = DeclaredMessage.Import(readResult.Buffer, _bytesPool);
            await pipeReader.CompleteAsync();

            return message;
        }

        public async ValueTask WriteMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default)
        {
            var signature = message.Certificate?.GetOmniSignature();
            if (signature == null)
            {
                return;
            }

            var status = _repository.Items.Get(signature);
            if (status == null)
            {
                return;
            }

            var filePath = Path.Combine(_cachePath, StringConverter.SignatureToString(signature));

            using var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
            var pipeWriter = PipeWriter.Create(fileStream);
            message.Export(pipeWriter, _bytesPool);
            await pipeWriter.CompleteAsync();
        }
    }
}
