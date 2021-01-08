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
    internal sealed partial class DeclaredMessagePublisherRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public DeclaredMessagePublisherRepository(string workingDirectory)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(workingDirectory)!);

            _database = new LiteDatabase(workingDirectory);
            this.Items = new DeclaredMessagePublisherItemRepository(_database);
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (_database.UserVersion <= 0)
            {
                var col = this.Items.GetCollection();
                col.EnsureIndex(x => x.Signature, false);
                _database.UserVersion = 1;
            }
        }

        public DeclaredMessagePublisherItemRepository Items { get; set; }

        public sealed class DeclaredMessagePublisherItemRepository
        {
            private readonly LiteDatabase _database;

            public DeclaredMessagePublisherItemRepository(LiteDatabase database)
            {
                _database = database;
            }

            public ILiteCollection<DeclaredMessagePublisherItemEntity> GetCollection()
            {
                var col = _database.GetCollection<DeclaredMessagePublisherItemEntity>("items");
                return col;
            }

            public IEnumerable<DeclaredMessagePublisherItem> FindAll()
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export());
            }

            public DeclaredMessagePublisherItem? Get(OmniSignature signature, string registrant)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                return col.FindOne(n => n.Signature == signatureEntity && n.Registrant == registrant).Export();
            }

            public void Add(DeclaredMessagePublisherItem item)
            {
                var itemEntity = DeclaredMessagePublisherItemEntity.Import(item);

                var col = this.GetCollection();

                if (col.Exists(n => n.Signature == itemEntity.Signature && n.Registrant == itemEntity.Registrant))
                {
                    return;
                }

                col.Insert(itemEntity);
            }

            public void Remove(OmniSignature signature, string registrant)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                col.DeleteMany(n => n.Signature == signatureEntity && n.Registrant == registrant);
            }
        }
    }
}
