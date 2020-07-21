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
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Repositories
{
    public sealed class WantContentRepository : AsyncDisposableBase, IWantContentRepository
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantContentRepositoryOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly LiteDatabase _database;

        private readonly AsyncLock _asyncLock = new AsyncLock();

        internal sealed class WantContentRepositoryFactory : IWantContentRepositoryFactory
        {
            public async ValueTask<IWantContentRepository> CreateAsync(WantContentRepositoryOptions options, IBytesPool bytesPool)
            {
                var result = new WantContentRepository(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IWantContentRepositoryFactory Factory { get; } = new WantContentRepositoryFactory();

        internal WantContentRepository(WantContentRepositoryOptions options, IBytesPool bytesPool)
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

        public ContentBlock GetContentBlock(OmniHash rootHash, OmniHash targetHash)
        {
            throw new NotImplementedException();
        }

        public void AddContentBlock(OmniHash rootHash, OmniHash targetHash)
        {
            throw new NotImplementedException();
        }

        public void RemoveContentBlock(OmniHash rootHash, OmniHash targetHash)
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
