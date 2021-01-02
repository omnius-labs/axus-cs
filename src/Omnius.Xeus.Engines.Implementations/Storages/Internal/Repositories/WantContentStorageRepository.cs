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
    internal sealed partial class WantContentStorageRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public WantContentStorageRepository(string workingDirectory)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(workingDirectory)!);

            _database = new LiteDatabase(workingDirectory);
            this.WantContentStatus = new WantContentStatusRepository(_database);
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (_database.UserVersion <= 0)
            {
                var wants = _database.GetCollection<WantContentStatusEntity>("wants");
                wants.EnsureIndex(x => x.Hash, true);
                _database.UserVersion = 1;
            }
        }

        public WantContentStatusRepository WantContentStatus { get; set; }

        public sealed class WantContentStatusRepository
        {
            private readonly LiteDatabase _database;

            public WantContentStatusRepository(LiteDatabase database)
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
