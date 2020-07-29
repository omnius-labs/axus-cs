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
using Omnius.Xeus.Service.Engines.Internal;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed class PushDeclaredMessageStorage : AsyncDisposableBase, IPushDeclaredMessageStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PushDeclaredMessageStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly LiteDatabase _database;

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

        internal PushDeclaredMessageStorage(PushDeclaredMessageStorageOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _database = new LiteDatabase(Path.Combine(_options.ConfigPath, "lite.db"));
        }

        internal async ValueTask InitAsync()
        {
            await this.MigrateAsync();
        }

        private async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (0 <= _database.UserVersion)
            {
                var pushes = _database.GetCollection<PushEntity>("pushes");
                pushes.EnsureIndex(x => x.Hash, true);
                _database.UserVersion = 1;
            }
        }

        protected override async ValueTask OnDisposeAsync()
        {
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

        public async ValueTask<bool> ContainsAsync(OmniSignature signature, DateTime since = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var hashString = this.ComputeHash(signature).ToString();

                var pushes = _database.GetCollection<PushEntity>("wants");
                if (!pushes.Exists(n => n.Hash == hashString)) return false;

                var id = $"$/declared_messages/{hashString}";
                var filename = hashString;

                var fileInfo = _database.FileStorage.FindById(id);
                var creationTime = fileInfo.Metadata["creation_time"].AsDateTime;
                if (since < creationTime) return true;
                return true;
            }
        }

        public async ValueTask AddAsync(DeclaredMessage message, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var signature = message.Certificate?.GetOmniSignature();
                if (signature == null) return;

                var hashString = this.ComputeHash(signature).ToString();

                var wants = _database.GetCollection<PushEntity>("pushes");
                if (!wants.Exists(n => n.Hash == hashString)) return;

                var id = $"$/declared_messages/{hashString}";
                var filename = hashString;
                var metadata = new BsonDocument(new Dictionary<string, BsonValue>() { { "creation_time", new BsonValue(message.CreationTime) } });

                using var liteStream = _database.FileStorage.OpenWrite(id, filename, metadata);
                var pipeWriter = PipeWriter.Create(liteStream);
                message.Export(pipeWriter, _bytesPool);
                await pipeWriter.CompleteAsync();
            }
        }

        public async ValueTask RemoveAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var hashString = this.ComputeHash(signature).ToString();

                var pushes = _database.GetCollection<PushEntity>("pushes");
                pushes.DeleteMany(n => n.Hash == hashString);
            }
        }

        public async ValueTask<DeclaredMessage?> ReadAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var hashString = this.ComputeHash(signature).ToString();

                var pushes = _database.GetCollection<PushEntity>("pushes");
                if (!pushes.Exists(n => n.Hash == hashString)) return null;

                var id = $"$/declared_messages/{hashString}";
                var filename = hashString;

                using var liteStream = _database.FileStorage.OpenRead(id);
                var pipeReader = PipeReader.Create(liteStream);
                var readResult = await pipeReader.ReadAsync(cancellationToken);
                var message = DeclaredMessage.Import(readResult.Buffer, _bytesPool);
                await pipeReader.CompleteAsync();

                return message;
            }
        }

        private OmniHash ComputeHash(OmniSignature signature)
        {
            var hub = new BytesHub(_bytesPool);
            signature.Export(hub.Writer, _bytesPool);
            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(hub.Reader.GetSequence()));
            return hash;
        }

        private sealed class PushEntity
        {
            public int Id { get; set; }
            public string? Hash { get; set; }
        }
    }
}
