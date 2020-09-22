using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Components.Storages.Internal.Models;
using Omnius.Xeus.Components.Storages.Internal.Repositories.Entities;

namespace Omnius.Xeus.Components.Storages.Internal.Repositories
{
    internal sealed partial class WantContentStorageRepository : IDisposable
    {
        private readonly LiteDatabase _database;

        public WantContentStorageRepository(string path)
        {
            _database = new LiteDatabase(path);
            this.WantStatus = new WantStatusRepository(_database);
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (0 <= _database.UserVersion)
            {
                var wants = _database.GetCollection<WantContentStatusEntity>("wants");
                wants.EnsureIndex(x => x.Hash, true);
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

            public IEnumerable<WantContentStatus> GetAll()
            {
                var col = _database.GetCollection<WantContentStatusEntity>("wants");
                return col.FindAll().Select(n => n.Export());
            }

            public WantContentStatus? Get(OmniHash rootHash)
            {
                var col = _database.GetCollection<WantContentStatusEntity>("wants");
                var param = OmniHashEntity.Import(rootHash);
                return col.FindOne(n => n.Hash == param)?.Export();
            }

            public void Add(WantContentStatus status)
            {
                var col = _database.GetCollection<WantContentStatusEntity>("wants");
                var param = WantContentStatusEntity.Import(status);
                col.Insert(param);
            }

            public void Remove(OmniHash rootHash)
            {
                var col = _database.GetCollection<WantContentStatusEntity>("wants");
                var param = OmniHashEntity.Import(rootHash);
                col.DeleteMany(n => n.Hash == param);
            }
        }
    }
}
