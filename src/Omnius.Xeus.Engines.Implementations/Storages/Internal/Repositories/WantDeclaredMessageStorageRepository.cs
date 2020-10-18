using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories
{
    internal sealed partial class WantDeclaredMessageStorageRepository : IDisposable
    {
        private readonly LiteDatabase _database;

        public WantDeclaredMessageStorageRepository(string path)
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
                var wants = _database.GetCollection<WantDeclaredMessageStatusEntity>("wants");
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

            public IEnumerable<WantDeclaredMessageStatus> GetAll()
            {
                var col = _database.GetCollection<WantDeclaredMessageStatusEntity>("wants");
                return col.FindAll().Select(n => n.Export());
            }

            public WantDeclaredMessageStatus? Get(OmniSignature signature)
            {
                var col = _database.GetCollection<WantDeclaredMessageStatusEntity>("wants");
                var param = OmniSignatureEntity.Import(signature);
                return col.FindOne(n => n.Signature == param)?.Export();
            }

            public void Add(WantDeclaredMessageStatus status)
            {
                var col = _database.GetCollection<WantDeclaredMessageStatusEntity>("wants");
                var param = WantDeclaredMessageStatusEntity.Import(status);
                col.Insert(param);
            }

            public void Remove(OmniSignature signature)
            {
                var col = _database.GetCollection<WantDeclaredMessageStatusEntity>("wants");
                var param = OmniSignatureEntity.Import(signature);
                col.DeleteMany(n => n.Signature == param);
            }
        }
    }
}
