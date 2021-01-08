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
    internal sealed partial class ContentSubscriberRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public ContentSubscriberRepository(string workingDirectory)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(workingDirectory)!);

            _database = new LiteDatabase(workingDirectory);
            this.Items = new ContentSubscriberItemRepository(_database);
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
                col.EnsureIndex(x => x.ContentHash, false);
                _database.UserVersion = 1;
            }
        }

        public ContentSubscriberItemRepository Items { get; set; }

        public sealed class ContentSubscriberItemRepository
        {
            private readonly LiteDatabase _database;

            public ContentSubscriberItemRepository(LiteDatabase database)
            {
                _database = database;
            }

            public ILiteCollection<ContentSubscriberItemEntity> GetCollection()
            {
                var col = _database.GetCollection<ContentSubscriberItemEntity>("items");
                return col;
            }

            public bool Exists(OmniHash contentHash)
            {
                var contentHashEntity = OmniHashEntity.Import(contentHash);

                var col = this.GetCollection();
                return col.Exists(n => n.ContentHash == contentHashEntity);
            }

            public IEnumerable<ContentSubscriberItem> FindAll()
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export());
            }

            public IEnumerable<ContentSubscriberItem> Find(OmniHash contentHash)
            {
                var contentHashEntity = OmniHashEntity.Import(contentHash);

                var col = this.GetCollection();
                return col.Find(n => n.ContentHash == contentHashEntity).Select(n => n.Export());
            }

            public ContentSubscriberItem? FindOne(OmniHash contentHash, string registrant)
            {
                var contentHashEntity = OmniHashEntity.Import(contentHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.ContentHash == contentHashEntity && n.Registrant == registrant)?.Export();
            }

            public void Insert(ContentSubscriberItem item)
            {
                var itemEntity = ContentSubscriberItemEntity.Import(item);

                var col = this.GetCollection();

                if (col.Exists(n => n.ContentHash == itemEntity.ContentHash && n.Registrant == itemEntity.Registrant))
                {
                    return;
                }

                col.Insert(itemEntity);
            }

            public void Delete(OmniHash contentHash, string registrant)
            {
                var contentHashEntity = OmniHashEntity.Import(contentHash);

                var col = this.GetCollection();
                col.DeleteMany(n => n.ContentHash == contentHashEntity && n.Registrant == registrant);
            }
        }
    }
}
