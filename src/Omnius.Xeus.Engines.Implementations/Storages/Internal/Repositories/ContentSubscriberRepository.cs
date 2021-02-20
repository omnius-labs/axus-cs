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
using Omnius.Xeus.Engines.Storages.Internal.Repositories.Helpers;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories
{
    internal sealed partial class ContentSubscriberRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public ContentSubscriberRepository(string filePath)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(filePath)!);

            _database = new LiteDatabase(filePath);
            this.Items = new SubscribedContentItemRepository(_database);
            this.DecodedItems = new DecodedContentItemRepository(_database);
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            await this.Items.MigrateAsync(cancellationToken);
            await this.DecodedItems.MigrateAsync(cancellationToken);
        }

        public SubscribedContentItemRepository Items { get; }

        public DecodedContentItemRepository DecodedItems { get; }

        public sealed class SubscribedContentItemRepository
        {
            private const string CollectionName = "items";

            private readonly LiteDatabase _database;

            private readonly AsyncReaderWriterLock _asyncLock = new();

            public SubscribedContentItemRepository(LiteDatabase database)
            {
                _database = database;
            }

            private ILiteCollection<SubscribedContentItemEntity> GetCollection()
            {
                var col = _database.GetCollection<SubscribedContentItemEntity>(CollectionName);
                return col;
            }

            internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    if (VersionHelper.GetVersion(_database, CollectionName) <= 0)
                    {
                        var col = this.GetCollection();
                        col.EnsureIndex(x => x.ContentHash, false);
                    }

                    VersionHelper.SetVersion(_database, CollectionName, 1);
                }
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

            public IEnumerable<SubscribedContentItem> FindAll()
            {
                using (_asyncLock.ReaderLock())
                {
                    var col = this.GetCollection();
                    return col.FindAll().Select(n => n.Export());
                }
            }

            public IEnumerable<SubscribedContentItem> Find(OmniHash contentHash)
            {
                using (_asyncLock.ReaderLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    return col.Find(n => n.ContentHash == contentHashEntity).Select(n => n.Export());
                }
            }

            public SubscribedContentItem? FindOne(OmniHash contentHash, string registrant)
            {
                using (_asyncLock.ReaderLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    return col.FindOne(n => n.ContentHash == contentHashEntity && n.Registrant == registrant)?.Export();
                }
            }

            public void Insert(SubscribedContentItem item)
            {
                using (_asyncLock.WriterLock())
                {
                    var itemEntity = SubscribedContentItemEntity.Import(item);

                    var col = this.GetCollection();

                    if (col.Exists(n => n.ContentHash == itemEntity.ContentHash && n.Registrant == itemEntity.Registrant)) return;

                    col.Insert(itemEntity);
                }
            }

            public void Delete(OmniHash contentHash, string registrant)
            {
                using (_asyncLock.WriterLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    col.DeleteMany(n => n.ContentHash == contentHashEntity && n.Registrant == registrant);
                }
            }
        }

        public sealed class DecodedContentItemRepository
        {
            private const string CollectionName = "decoded_items";

            private readonly LiteDatabase _database;

            private readonly AsyncReaderWriterLock _asyncLock = new();

            public DecodedContentItemRepository(LiteDatabase database)
            {
                _database = database;
            }

            private ILiteCollection<DecodedContentItemEntity> GetCollection()
            {
                var col = _database.GetCollection<DecodedContentItemEntity>(CollectionName);
                return col;
            }

            internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    if (VersionHelper.GetVersion(_database, CollectionName) <= 0)
                    {
                        var col = this.GetCollection();
                        col.EnsureIndex(x => x.ContentHash, true);
                    }

                    VersionHelper.SetVersion(_database, CollectionName, 1);
                }
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

            public IEnumerable<DecodedContentItem> FindAll()
            {
                using (_asyncLock.ReaderLock())
                {
                    var col = this.GetCollection();
                    return col.FindAll().Select(n => n.Export());
                }
            }

            public DecodedContentItem? FindOne(OmniHash contentHash)
            {
                using (_asyncLock.ReaderLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    return col.FindOne(n => n.ContentHash == contentHashEntity)?.Export();
                }
            }

            public void Insert(DecodedContentItem item)
            {
                using (_asyncLock.WriterLock())
                {
                    var itemEntity = DecodedContentItemEntity.Import(item);

                    var col = this.GetCollection();

                    if (col.Exists(n => n.ContentHash == itemEntity.ContentHash)) return;

                    col.Insert(itemEntity);
                }
            }

            public void Delete(OmniHash contentHash)
            {
                using (_asyncLock.WriterLock())
                {
                    var contentHashEntity = OmniHashEntity.Import(contentHash);

                    var col = this.GetCollection();
                    col.DeleteMany(n => n.ContentHash == contentHashEntity);
                }
            }
        }
    }
}
