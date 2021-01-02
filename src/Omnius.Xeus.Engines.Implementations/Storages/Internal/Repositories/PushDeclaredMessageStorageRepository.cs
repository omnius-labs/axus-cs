using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories
{
    internal sealed partial class PushDeclaredMessageStorageRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public PushDeclaredMessageStorageRepository(string workingDirectory)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(workingDirectory)!);

            _database = new LiteDatabase(workingDirectory);
            this.PushDeclaredMessageStatus = new PushDeclaredMessageStatusRepository(_database);
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (_database.UserVersion <= 0)
            {
                var wants = _database.GetCollection<PushDeclaredMessageStatusEntity>("pushes");
                wants.EnsureIndex(x => x.Signature, true);
                _database.UserVersion = 1;
            }
        }

        public PushDeclaredMessageStatusRepository PushDeclaredMessageStatus { get; set; }

        public sealed class PushDeclaredMessageStatusRepository
        {
            private readonly LiteDatabase _database;

            public PushDeclaredMessageStatusRepository(LiteDatabase database)
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
