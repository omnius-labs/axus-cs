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
    public sealed partial class WantDeclaredMessageStorage : AsyncDisposableBase, IWantDeclaredMessageStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantDeclaredMessageStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly WantDeclaredMessageStorageRepository _repository;
        private readonly string _cachePath;

        internal sealed class WantDeclaredMessageStorageFactory : IWantDeclaredMessageStorageFactory
        {
            public async ValueTask<IWantDeclaredMessageStorage> CreateAsync(WantDeclaredMessageStorageOptions options, IBytesPool bytesPool)
            {
                var result = new WantDeclaredMessageStorage(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IWantDeclaredMessageStorageFactory Factory { get; } = new WantDeclaredMessageStorageFactory();

        private WantDeclaredMessageStorage(WantDeclaredMessageStorageOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _repository = new WantDeclaredMessageStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "want-declared-message-storage.db"));
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

        public async ValueTask<WantDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<OmniSignature>();

            foreach (var status in _repository.WantDeclaredMessageStatus.GetAll())
            {
                results.Add(status.Signature);
            }

            return results;
        }

        public async ValueTask<bool> ContainsMessageAsync(OmniSignature signature, DateTime since = default)
        {
            var status = _repository.WantDeclaredMessageStatus.Get(signature);
            if (status == null)
            {
                return false;
            }

            return true;
        }

        public async ValueTask RegisterWantMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            _repository.WantDeclaredMessageStatus.Add(new WantDeclaredMessageStatus(signature, DateTime.MinValue));
        }

        public async ValueTask UnregisterWantMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            _repository.WantDeclaredMessageStatus.Remove(signature);
        }

        public async ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var status = _repository.WantDeclaredMessageStatus.Get(signature);
            if (status == null)
            {
                return null;
            }

            return status.CreationTime;
        }

        public async ValueTask<DeclaredMessage?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var status = _repository.WantDeclaredMessageStatus.Get(signature);
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

            var status = _repository.WantDeclaredMessageStatus.Get(signature);
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
