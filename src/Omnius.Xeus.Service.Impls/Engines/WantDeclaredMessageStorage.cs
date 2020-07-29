using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed class WantDeclaredMessageStorage : AsyncDisposableBase, IWantDeclaredMessageStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantDeclaredMessageStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly Repository _repository;

        private readonly AsyncLock _asyncLock = new AsyncLock();

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

        internal WantDeclaredMessageStorage(WantDeclaredMessageStorageOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _repository = new Repository(Path.Combine(_options.ConfigPath, "lite.db"));
        }

        internal async ValueTask InitAsync()
        {
            await _repository.MigrateAsync();
        }

        protected override async ValueTask OnDisposeAsync()
        {

        }

        public async ValueTask<WantDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                throw new NotImplementedException();
            }
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                throw new NotImplementedException();
            }
        }

        public ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> ContainsAsync(OmniSignature signature, DateTime since = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(signature);
                if (status == null) return false;
                return true;
            }
        }

        public async ValueTask AddAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _repository.WantStatus.Add(new WantStatus() { Signature = signature });
            }
        }

        public async ValueTask RemoveAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _repository.WantStatus.Remove(signature);
            }
        }

        public async ValueTask<DeclaredMessage?> ReadAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(signature);
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

        public async ValueTask WriteAsync(DeclaredMessage message, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var signature = message.Certificate?.GetOmniSignature();
                if (signature == null) return;

                _repository.WantStatus.Add(new WantStatus() { Signature = signature });

                var filePath = Path.Combine(_options.ConfigPath, "cache", SignatureToString(signature));

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
                var pipeWriter = PipeWriter.Create(fileStream);
                message.Export(pipeWriter, _bytesPool);
                await pipeWriter.CompleteAsync();
            }
        }

        private static string SignatureToString(OmniSignature signature)
        {
            var hub = new BytesHub(BytesPool.Shared);
            signature.Export(hub.Writer, BytesPool.Shared);
            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(hub.Reader.GetSequence()));
            return hash.ToString(ConvertStringType.Base16);
        }

        private sealed class WantStatus
        {
            public OmniSignature? Signature { get; set; }
        }

        private sealed class Repository
        {
            private readonly LiteDatabase _database;

            public Repository(string path)
            {
                _database = new LiteDatabase(path);
                this.WantStatus = new WantStatusRepository(_database);
            }

            public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
            {
                if (0 <= _database.UserVersion)
                {
                    var wants = _database.GetCollection<WantStatusEntity>("wants-status");
                    wants.EnsureIndex(x => x.Signature, true);
                    _database.UserVersion = 1;
                }
            }

            public WantStatusRepository WantStatus { get; set; }

            public sealed class WantStatusRepository
            {
                private readonly LiteDatabase _database;

                public WantStatusRepository(LiteDatabase database)
                {
                    _database = database;
                }

                public IEnumerable<WantStatus> GetAll()
                {
                    throw new NotImplementedException();
                }

                public WantStatus? Get(OmniSignature signature)
                {
                    throw new NotImplementedException();
                }

                public void Add(WantStatus status)
                {
                    throw new NotImplementedException();
                }

                public void Remove(OmniSignature signature)
                {
                    throw new NotImplementedException();
                }
            }

            private sealed class WantStatusEntity
            {
                public int Id { get; set; }
                public OmniSignatureEntity? Signature { get; set; }
            }

            private class OmniSignatureEntity
            {
                public string? Name { get; set; }
                public OmniHashEntity? Hash { get; set; }
            }

            private class OmniHashEntity
            {
                public int AlgorithmType { get; set; }
                public byte[]? Value { get; set; }
            }
        }
    }
}
