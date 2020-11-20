using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Interactors.Storages.Internal.Repositories.Entities;

namespace Omnius.Xeus.Interactors.Storages.Internal.Repositories
{
    internal sealed partial class UserProfileStorageRepository : IDisposable
    {
        private readonly LiteDatabase _database;

        public UserProfileStorageRepository(string path)
        {
            _database = new LiteDatabase(path);
            this.SubscribedSignatures = new SubscribedSignaturesRepository(_database);
        }

        public void Dispose()
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (_database.UserVersion >= 0)
            {
                var wants = _database.GetCollection<OmniSignatureEntity>();
                wants.EnsureIndex(x => x, true);
                _database.UserVersion = 1;
            }
        }

        public SubscribedSignaturesRepository SubscribedSignatures { get; set; }

        public sealed class SubscribedSignaturesRepository
        {
            public const string TableName = "subscribed-signatures";

            private readonly LiteDatabase _database;

            public SubscribedSignaturesRepository(LiteDatabase database)
            {
                _database = database;
            }

            public IEnumerable<OmniSignature> GetAll()
            {
                var col = _database.GetCollection<OmniSignatureEntity>(TableName);
                return col.FindAll().Select(n => n.Export());
            }

            public bool Contains(OmniSignature signature)
            {
                var col = _database.GetCollection<OmniSignatureEntity>(TableName);
                var param = OmniSignatureEntity.Import(signature);
                return col.Exists(n => n == param);
            }

            public void Add(OmniSignature signature)
            {
                var col = _database.GetCollection<OmniSignatureEntity>(TableName);
                col.Upsert(OmniSignatureEntity.Import(signature));
            }

            public void Remove(OmniSignature signature)
            {
                var col = _database.GetCollection<OmniSignatureEntity>(TableName);
                var param = OmniSignatureEntity.Import(signature);
                col.DeleteMany(n => n == param);
            }
        }
    }
}
