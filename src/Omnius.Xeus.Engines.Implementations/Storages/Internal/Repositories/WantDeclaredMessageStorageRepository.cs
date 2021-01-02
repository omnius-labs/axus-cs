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
    internal sealed partial class WantDeclaredMessageStorageRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public WantDeclaredMessageStorageRepository(string workingDirectory)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(workingDirectory)!);

            _database = new LiteDatabase(workingDirectory);
            this.WantDeclaredMessageStatus = new WantDeclaredMessageStatusRepository(_database);
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (_database.UserVersion <= 0)
            {
                var wants = _database.GetCollection<WantDeclaredMessageStatusEntity>("wants");
                wants.EnsureIndex(x => x.Signature, true);
                _database.UserVersion = 1;
            }
        }

        public WantDeclaredMessageStatusRepository WantDeclaredMessageStatus { get; set; }

        public sealed class WantDeclaredMessageStatusRepository
        {
            private readonly LiteDatabase _database;

            public WantDeclaredMessageStatusRepository(LiteDatabase database)
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
