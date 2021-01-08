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
    internal sealed class ContentPublisherRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public ContentPublisherRepository(string workingDirectory)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(workingDirectory)!);

            _database = new LiteDatabase(workingDirectory);
            this.Items = new ContentPublisherItemRepository(_database);
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
                col.EnsureIndex(x => x.FilePath, false);
                _database.UserVersion = 1;
            }
        }

        public ContentPublisherItemRepository Items { get; set; }

        public sealed class ContentPublisherItemRepository
        {
            private readonly LiteDatabase _database;

            public ContentPublisherItemRepository(LiteDatabase database)
            {
                _database = database;
            }

            public ILiteCollection<ContentPublisherItemEntity> GetCollection()
            {
                var col = _database.GetCollection<ContentPublisherItemEntity>("items");
                return col;
            }

            public bool Exists(OmniHash contentHash)
            {
                var contentHashEntity = OmniHashEntity.Import(contentHash);

                var col = this.GetCollection();
                return col.Exists(n => n.ContentHash == contentHashEntity);
            }

            public bool Exists(string filePath)
            {
                var col = this.GetCollection();
                return col.Exists(n => n.FilePath == filePath);
            }

            public IEnumerable<ContentPublisherItem> FindAll()
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export());
            }

            public IEnumerable<ContentPublisherItem> Find(OmniHash contentHash)
            {
                var contentHashEntity = OmniHashEntity.Import(contentHash);

                var col = this.GetCollection();
                return col.Find(n => n.ContentHash == contentHashEntity).Select(n => n.Export());
            }

            public ContentPublisherItem? FindOne(OmniHash contentHash, string registrant)
            {
                var contentHashEntity = OmniHashEntity.Import(contentHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.ContentHash == contentHashEntity && n.Registrant == registrant)?.Export();
            }

            public ContentPublisherItem? FindOne(string filePath, string registrant)
            {
                var col = this.GetCollection();
                return col.FindOne(n => n.FilePath == filePath && n.Registrant == registrant)?.Export();
            }

            public void Insert(ContentPublisherItem item)
            {
                var itemEntity = ContentPublisherItemEntity.Import(item);

                var col = this.GetCollection();

                if (col.Exists(n => n.FilePath == itemEntity.FilePath && n.Registrant == itemEntity.Registrant))
                {
                    return;
                }

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

            public void Delete(string filePath, string registrant)
            {
                var col = this.GetCollection();
                col.DeleteMany(n => n.FilePath == filePath && n.Registrant == registrant);
            }
        }
    }
}
