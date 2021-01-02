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
    internal sealed class PushContentStorageRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public PushContentStorageRepository(string workingDirectory)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(workingDirectory)!);

            _database = new LiteDatabase(workingDirectory);
            this.PushContentStatus = new PushContentStatusRepository(_database);
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (_database.UserVersion <= 0)
            {
                var wants = _database.GetCollection<PushContentStatusEntity>("pushes");
                wants.EnsureIndex(x => x.Hash, false);
                wants.EnsureIndex(x => x.FilePath, true);
                _database.UserVersion = 1;
            }
        }

        public PushContentStatusRepository PushContentStatus { get; set; }

        public sealed class PushContentStatusRepository
        {
            private readonly LiteDatabase _database;

            public PushContentStatusRepository(LiteDatabase database)
            {
                _database = database;
            }

            public IEnumerable<PushContentStatus> GetAll()
            {
                var col = _database.GetCollection<PushContentStatusEntity>("pushes");
                return col.FindAll().Select(n => n.Export());
            }

            public PushContentStatus? Get(OmniHash rootHash)
            {
                var col = _database.GetCollection<PushContentStatusEntity>("pushes");
                var param = OmniHashEntity.Import(rootHash);
                return col.FindOne(n => n.Hash == param)?.Export();
            }

            public PushContentStatus? Get(string filePath)
            {
                var col = _database.GetCollection<PushContentStatusEntity>("pushes");
                return col.FindOne(n => n.FilePath == filePath)?.Export();
            }

            public void Add(PushContentStatus status)
            {
                var col = _database.GetCollection<PushContentStatusEntity>("pushes");
                var param = PushContentStatusEntity.Import(status);
                col.Insert(param);
            }

            public void Remove(OmniHash rootHash)
            {
                var col = _database.GetCollection<PushContentStatusEntity>("pushes");
                var param = OmniHashEntity.Import(rootHash);
                col.DeleteMany(n => n.Hash == param);
            }

            public void Remove(string filePath)
            {
                var col = _database.GetCollection<PushContentStatusEntity>("pushes");
                col.DeleteMany(n => n.FilePath == filePath);
            }
        }
    }
}
