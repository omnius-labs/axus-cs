using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities;
using Omnius.Xeus.Utils.Extentions;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories
{
    internal sealed class ContentPublisherRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public ContentPublisherRepository(string dirPath)
        {
            DirectoryHelper.CreateDirectory(dirPath);

            _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
            this.Items = new PublishedContentItemRepository(_database);
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            await this.Items.MigrateAsync(cancellationToken);
        }

        public PublishedContentItemRepository Items { get; }

        public sealed class PublishedContentItemRepository
        {
            private const string CollectionName = "published_items";

            private readonly LiteDatabase _database;

            private readonly AsyncReaderWriterLock _asyncLock = new();

            public PublishedContentItemRepository(LiteDatabase database)
            {
                _database = database;
            }

            internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    if (_database.GetDocumentVersion(CollectionName) <= 0)
                    {
                        var col = this.GetCollection();
                        col.EnsureIndex(x => x.ContentHash, false);
                        col.EnsureIndex(x => x.FilePath, false);
                    }

                    _database.SetDocumentVersion(CollectionName, 1);
                }
            }

            private ILiteCollection<PublishedContentItemEntity> GetCollection()
            {
                var col = _database.GetCollection<PublishedContentItemEntity>(CollectionName);
                return col;
            }

            public bool Exists(OmniHash contentHash)
            {
                using (_asyncLock.ReaderLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    return col.Exists(n => n.ContentHash == contentHashEntity);
                }
            }

            public bool Exists(string filePath)
            {
                using (_asyncLock.ReaderLock())
                {
                    var col = this.GetCollection();
                    return col.Exists(n => n.FilePath == filePath);
                }
            }

            public IEnumerable<PublishedContentItem> FindAll()
            {
                using (_asyncLock.ReaderLock())
                {
                    var col = this.GetCollection();
                    return col.FindAll().Select(n => n.Export()).ToArray();
                }
            }

            public IEnumerable<PublishedContentItem> Find(OmniHash contentHash)
            {
                using (_asyncLock.ReaderLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    return col.Find(n => n.ContentHash == contentHashEntity).Select(n => n.Export()).ToArray();
                }
            }

            public PublishedContentItem? FindOne(string filePath, string registrant)
            {
                using (_asyncLock.ReaderLock())
                {
                    var col = this.GetCollection();
                    return col.FindOne(n => n.FilePath == filePath && n.Registrant == registrant)?.Export();
                }
            }

            public PublishedContentItem? FindOne(OmniHash contentHash, string registrant)
            {
                using (_asyncLock.ReaderLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    return col.FindOne(n => n.ContentHash == contentHashEntity && n.FilePath == null && n.Registrant == registrant)?.Export();
                }
            }

            public void Upsert(PublishedContentItem item)
            {
                using (_asyncLock.WriterLock())
                {
                    var itemEntity = PublishedContentItemEntity.Import(item);

                    var col = this.GetCollection();

                    if (item.FilePath is not null)
                    {
                        col.DeleteMany(n => n.FilePath == item.FilePath && n.Registrant == item.Registrant);
                    }
                    else
                    {
                        col.DeleteMany(n => n.ContentHash == itemEntity.ContentHash && n.FilePath == null && n.Registrant == item.Registrant);
                    }

                    col.Insert(itemEntity);
                }
            }

            public void Delete(OmniHash contentHash, string registrant)
            {
                using (_asyncLock.WriterLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    col.DeleteMany(n => n.ContentHash == contentHashEntity && n.FilePath == null && n.Registrant == registrant);
                }
            }

            public void Delete(string filePath, string registrant)
            {
                using (_asyncLock.WriterLock())
                {
                    var col = this.GetCollection();
                    col.DeleteMany(n => n.FilePath == filePath && n.Registrant == registrant);
                }
            }
        }
    }
}
