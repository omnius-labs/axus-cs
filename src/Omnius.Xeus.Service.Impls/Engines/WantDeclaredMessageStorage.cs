using System.Security.Authentication;
using System.Security.Cryptography;
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
        private readonly string _cachePath;

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

        private WantDeclaredMessageStorage(WantDeclaredMessageStorageOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _repository = new Repository(Path.Combine(_options.ConfigPath, "lite.db"));
            _cachePath = Path.Combine(_options.ConfigPath, "cache");
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

        public async ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.GetAll();
                if (status == null) return Enumerable.Empty<OmniSignature>();
                return status.Select(n => n.Signature).ToArray();
            }
        }

        public async ValueTask<bool> ContainsMessageAsync(OmniSignature signature, DateTime since = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(signature);
                if (status == null) return false;
                return true;
            }
        }

        public async ValueTask RegisterWantMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _repository.WantStatus.Add(new WantStatus(signature, DateTime.MinValue));
            }
        }

        public async ValueTask UnregisterWantMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _repository.WantStatus.Remove(signature);
            }
        }

        public async ValueTask<DeclaredMessage?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(signature);
                if (status == null) return null;

                var filePath = Path.Combine(_cachePath, SignatureToString(signature));

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
                var pipeReader = PipeReader.Create(fileStream);
                var readResult = await pipeReader.ReadAsync(cancellationToken);
                var message = DeclaredMessage.Import(readResult.Buffer, _bytesPool);
                await pipeReader.CompleteAsync();

                return message;
            }
        }

        public async ValueTask WriteMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var signature = message.Certificate?.GetOmniSignature();
                if (signature == null) return;

                var status = _repository.WantStatus.Get(signature);
                if (status == null) return;

                var filePath = Path.Combine(_cachePath, SignatureToString(signature));

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
            public WantStatus(OmniSignature signature, DateTime creationTime)
            {
                this.Signature = signature;
                this.CreationTime = creationTime;
            }

            public OmniSignature Signature { get; }
            public DateTime CreationTime { get; }
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
                    var wants = _database.GetCollection<WantStatusEntity>("wants");
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
                    var col = _database.GetCollection<WantStatusEntity>("wants");
                    return col.FindAll().Select(n => n.Export());
                }

                public WantStatus? Get(OmniSignature signature)
                {
                    var col = _database.GetCollection<WantStatusEntity>("wants");
                    var param = OmniSignatureEntity.Import(signature);
                    return col.FindOne(n => n.Signature == param)?.Export();
                }

                public void Add(WantStatus status)
                {
                    var col = _database.GetCollection<WantStatusEntity>("wants");
                    var param = WantStatusEntity.Import(status);

                    col.Insert(param);
                }

                public void Remove(OmniSignature signature)
                {
                    var col = _database.GetCollection<WantStatusEntity>("wants");
                    var param = OmniSignatureEntity.Import(signature);
                    col.DeleteMany(n => n.Signature == param);
                }
            }

            private sealed class WantStatusEntity
            {
                public int Id { get; set; }
                public OmniSignatureEntity? Signature { get; set; }
                public DateTime CreationTime { get; set; }

                public static WantStatusEntity Import(WantStatus value)
                {
                    return new WantStatusEntity() { Signature = OmniSignatureEntity.Import(value.Signature), CreationTime = value.CreationTime };
                }

                public WantStatus Export()
                {
                    return new WantStatus(this.Signature!.Export(), this.CreationTime);
                }
            }

            private class OmniSignatureEntity
            {
                public string? Name { get; set; }
                public OmniHashEntity? Hash { get; set; }

                public static OmniSignatureEntity Import(OmniSignature value)
                {
                    return new OmniSignatureEntity() { Name = value.Name, Hash = OmniHashEntity.Import(value.Hash) };
                }

                public OmniSignature Export()
                {
                    return new OmniSignature(this.Name!, this.Hash!.Export());
                }
            }

            private class OmniHashEntity
            {
                public int AlgorithmType { get; set; }
                public byte[]? Value { get; set; }

                public static OmniHashEntity Import(OmniHash value)
                {
                    return new OmniHashEntity() { AlgorithmType = (int)value.AlgorithmType, Value = value.Value.ToArray() };
                }

                public OmniHash Export()
                {
                    return new OmniHash((OmniHashAlgorithmType)this.AlgorithmType, this.Value);
                }
            }
        }
    }
}
