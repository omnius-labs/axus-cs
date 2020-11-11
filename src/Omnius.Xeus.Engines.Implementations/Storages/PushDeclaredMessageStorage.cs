using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories;

namespace Omnius.Xeus.Engines.Storages
{
    public sealed partial class PushDeclaredMessageStorage : AsyncDisposableBase, IPushDeclaredMessageStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PushDeclaredMessageStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly PushDeclaredMessageStorageRepository _repository;

        private readonly AsyncLock _asyncLock = new AsyncLock();

        internal sealed class PushDeclaredMessageStorageFactory : IPushDeclaredMessageStorageFactory
        {
            public async ValueTask<IPushDeclaredMessageStorage> CreateAsync(PushDeclaredMessageStorageOptions options,
                IBytesPool bytesPool)
            {
                var result = new PushDeclaredMessageStorage(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IPushDeclaredMessageStorageFactory Factory { get; } = new PushDeclaredMessageStorageFactory();

        private PushDeclaredMessageStorage(PushDeclaredMessageStorageOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _repository = new PushDeclaredMessageStorageRepository(Path.Combine(_options.ConfigPath, "database"));
        }

        internal async ValueTask InitAsync()
        {
            await _repository.MigrateAsync();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _repository.Dispose();
        }

        public async ValueTask<PushDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                throw new NotImplementedException();
            }
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> ContainsMessageAsync(OmniSignature signature, DateTime since = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.PushStatus.Get(signature);
                if (status == null) return false;
                return true;
            }
        }

        public async ValueTask RegisterPushMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var signature = message.Certificate?.GetOmniSignature();
                if (signature == null) return;

                _repository.PushStatus.Add(new PushDeclaredMessageStatus(signature, message.CreationTime.ToDateTime()));

                var filePath = Path.Combine(_options.ConfigPath, "cache", SignatureToString(signature));

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
                var pipeWriter = PipeWriter.Create(fileStream);
                message.Export(pipeWriter, _bytesPool);
                await pipeWriter.CompleteAsync();
            }
        }

        public async ValueTask UnregisterPushMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _repository.PushStatus.Remove(signature);
            }
        }

        public async ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.PushStatus.Get(signature);
                if (status == null) return null;

                return status.CreationTime;
            }
        }

        public async ValueTask<DeclaredMessage?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.PushStatus.Get(signature);
                if (status == null) return null;

                var filePath = Path.Combine(_options.ConfigPath, "cache", SignatureToString(signature));

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
                var pipeReader = PipeReader.Create(fileStream);
                var readResult = await pipeReader.ReadAsync(cancellationToken);
                var message = DeclaredMessage.Import(readResult.Buffer, _bytesPool);
                await pipeReader.CompleteAsync();

                return message;
            }
        }

        private static string SignatureToString(OmniSignature signature)
        {
            var hub = new BytesHub(BytesPool.Shared);
            signature.Export(hub.Writer, BytesPool.Shared);
            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(hub.Reader.GetSequence()));
            return hash.ToString(ConvertStringType.Base16);
        }
    }
}
