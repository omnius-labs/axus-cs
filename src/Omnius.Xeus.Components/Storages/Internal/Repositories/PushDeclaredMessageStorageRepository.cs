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
    internal sealed partial class PushDeclaredMessageStorageRepository : IDisposable
    {
        private readonly LiteDatabase _database;

        public PushDeclaredMessageStorageRepository(string path)
        {
            _database = new LiteDatabase(path);
            this.PushStatus = new PushStatusRepository(_database);
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (0 <= _database.UserVersion)
            {
                var wants = _database.GetCollection<PushDeclaredMessageStatusEntity>("pushes");
                wants.EnsureIndex(x => x.Signature, true);
                _database.UserVersion = 1;
            }
        }

        public PushStatusRepository PushStatus { get; set; }

        public sealed class PushStatusRepository
        {
            private readonly LiteDatabase _database;

            public PushStatusRepository(LiteDatabase database)
            {
                _database = database;
            }

            public IEnumerable<PushDeclaredMessageStatus> GetAll()
            {
                var col = _database.GetCollection<PushDeclaredMessageStatusEntity>("pushes");
                return col.FindAll().Select(n => n.Export());
            }

            public PushDeclaredMessageStatus? Get(OmniSignature signature)
            {
                var col = _database.GetCollection<PushDeclaredMessageStatusEntity>("pushes");
                var param = OmniSignatureEntity.Import(signature);
                return col.FindOne(n => n.Signature == param).Export();
            }

            public void Add(PushDeclaredMessageStatus status)
            {
                var col = _database.GetCollection<PushDeclaredMessageStatusEntity>("pushes");
                col.Upsert(PushDeclaredMessageStatusEntity.Import(status));
            }

            public void Remove(OmniSignature signature)
            {
                var col = _database.GetCollection<PushDeclaredMessageStatusEntity>("pushes");
                var param = OmniSignatureEntity.Import(signature);
                col.DeleteMany(n => n.Signature == param);
            }
        }
    }
}
