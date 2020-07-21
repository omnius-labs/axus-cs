using System.ComponentModel.Design;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Service.Engines;
using LiteDB;

namespace Omnius.Xeus.Service.Repositories
{
    public sealed class WantDeclaredMessageRepository : AsyncDisposableBase, IWantDeclaredMessageRepository
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantDeclaredMessageRepositoryOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly LiteDatabase _database;

        private readonly AsyncLock _asyncLock = new AsyncLock();

        internal sealed class WantDeclaredMessageRepositoryFactory : IWantDeclaredMessageRepositoryFactory
        {
            public async ValueTask<IWantDeclaredMessageRepository> CreateAsync(WantDeclaredMessageRepositoryOptions options, IBytesPool bytesPool)
            {
                var result = new WantDeclaredMessageRepository(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IWantDeclaredMessageRepositoryFactory Factory { get; } = new WantDeclaredMessageRepositoryFactory();

        internal WantDeclaredMessageRepository(WantDeclaredMessageRepositoryOptions options, IBytesPool bytesPool)
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
            var wants = _database.GetCollection<WantEntity>("wants");
            wants.EnsureIndex(x => x.Hash, true);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _database.Dispose();
        }

        public IEnumerable<OmniHash> GetWants()
        {
            var result = new List<OmniHash>();

            var wants = _database.GetCollection<WantEntity>("wants");

            foreach (var entity in wants.FindAll())
            {
                if (entity.Hash != null) result.Add(entity.Hash.Export());
            }

            return result;
        }

        public void AddWant(OmniHash hash)
        {
            var wants = _database.GetCollection<WantEntity>("wants");
            wants.Insert(new WantEntity() { Hash = HashEntity.Import(hash) });
        }

        public void RemoveWant(OmniHash hash)
        {
            var wants = _database.GetCollection<WantEntity>("wants");
            var hashEntity = HashEntity.Import(hash);
            wants.DeleteMany(n => n.Hash == hashEntity);
        }

        public DeclaredMessage GetDeclaredMessage(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public void GetDeclaredMessageCreationTime(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public void AddDeclaredMessage(DeclaredMessage message)
        {
            throw new NotImplementedException();
        }

        public void RemoveDeclaredMessage(OmniHash hash)
        {
            throw new NotImplementedException();
        }

        public sealed class WantEntity
        {
            public int Id { get; set; }
            public HashEntity? Hash { get; set; }
        }

        public sealed class HashEntity
        {
            public static HashEntity Import(OmniHash omniHash)
            {
                return new HashEntity()
                {
                    AlgorithmType = (int)omniHash.AlgorithmType,
                    Value = omniHash.Value.ToArray()
                };
            }

            public OmniHash Export()
            {
                return new OmniHash((OmniHashAlgorithmType)this.AlgorithmType, this.Value);
            }

            public int AlgorithmType { get; set; }
            public byte[]? Value { get; set; }
        }
    }
}
