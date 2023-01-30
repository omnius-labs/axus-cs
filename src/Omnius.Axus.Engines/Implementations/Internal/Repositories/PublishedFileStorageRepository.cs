using LiteDB;
using Omnius.Axus.Engines.Internal.Entities;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Engines.Internal.Repositories;

internal sealed class PublishedFileStorageRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public PublishedFileStorageRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.FileItems = new PublishedFileItemRepository(_database);
        this.InternalBlockItems = new PublishedInternalBlockItemRepository(_database);
        this.ExternalBlockItems = new PublishedExternalBlockItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.FileItems.MigrateAsync(cancellationToken);
    }

    public PublishedFileItemRepository FileItems { get; }
    public PublishedInternalBlockItemRepository InternalBlockItems { get; }
    public PublishedExternalBlockItemRepository ExternalBlockItems { get; }

    public sealed class PublishedFileItemRepository
    {
        private const string CollectionName = "published_file_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public PublishedFileItemRepository(LiteDatabase database)
        {
            _database = database;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
                if (_database.GetDocumentVersion(CollectionName) <= 0)
                {
                    var col = this.GetCollection();
                    col.EnsureIndex(x => x.RootHash, false);
                    col.EnsureIndex(x => x.FilePath, false);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<PublishedFileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<PublishedFileItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Exists(n => n.RootHash == rootHashEntity);
            }
        }

        public bool Exists(string filePath)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.Exists(n => n.FilePath == filePath);
            }
        }

        public IEnumerable<PublishedFileItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<PublishedFileItem> Find(string zone)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.Find(n => n.Authors!.Contains(zone)).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<PublishedFileItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public PublishedFileItem? FindOne(string filePath)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindOne(n => n.FilePath == filePath)?.Export();
            }
        }

        public PublishedFileItem? FindOne(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.RootHash == rootHashEntity && n.FilePath == null)?.Export();
            }
        }

        public void Upsert(PublishedFileItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = PublishedFileItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                if (item.FilePath is not null)
                {
                    col.DeleteMany(n => n.FilePath == item.FilePath);
                }
                else
                {
                    col.DeleteMany(n => n.RootHash == itemEntity.RootHash && n.FilePath == null);
                }

                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                col.DeleteMany(n => n.RootHash == rootHashEntity && n.FilePath == null);
            }
        }

        public void Delete(string filePath)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                col.DeleteMany(n => n.FilePath == filePath);
            }
        }
    }

    public sealed class PublishedInternalBlockItemRepository
    {
        private const string CollectionName = "published_internal_block_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public PublishedInternalBlockItemRepository(LiteDatabase database)
        {
            _database = database;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
                if (_database.GetDocumentVersion(CollectionName) <= 0)
                {
                    var col = this.GetCollection();
                    col.EnsureIndex(x => x.RootHash, false);
                    col.EnsureIndex(x => x.BlockHash, false);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<PublishedInternalBlockItemEntity> GetCollection()
        {
            var col = _database.GetCollection<PublishedInternalBlockItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(OmniHash rootHash, OmniHash blockHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);
                var blockHashEntity = OmniHashEntity.Import(blockHash);

                var col = this.GetCollection();
                return col.Exists(n => n.BlockHash == blockHashEntity && n.RootHash == rootHashEntity);
            }
        }

        public IEnumerable<PublishedInternalBlockItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<PublishedInternalBlockItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<PublishedInternalBlockItem> Find(OmniHash rootHash, OmniHash blockHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);
                var blockHashEntity = OmniHashEntity.Import(blockHash);

                var col = this.GetCollection();
                return col.Find(n => n.BlockHash == blockHashEntity && n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public void Upsert(PublishedInternalBlockItem item)
        {
            this.UpsertBulk(new[] { item });
        }

        public void UpsertBulk(IEnumerable<PublishedInternalBlockItem> items)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();

                _database.BeginTrans();

                foreach (var item in items)
                {
                    var itemEntity = PublishedInternalBlockItemEntity.Import(item);

                    col.DeleteMany(n =>
                        n.BlockHash == itemEntity.BlockHash
                        && n.RootHash == itemEntity.RootHash
                        && n.Depth == itemEntity.Depth
                        && n.Order == itemEntity.Order);
                    col.Insert(itemEntity);
                }

                _database.Commit();
            }
        }

        public void Delete(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                col.DeleteMany(n => n.RootHash == rootHashEntity);
            }
        }
    }

    public sealed class PublishedExternalBlockItemRepository
    {
        private const string CollectionName = "published_external_block_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public PublishedExternalBlockItemRepository(LiteDatabase database)
        {
            _database = database;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
                if (_database.GetDocumentVersion(CollectionName) <= 0)
                {
                    var col = this.GetCollection();
                    col.EnsureIndex(x => x.RootHash, false);
                    col.EnsureIndex(x => x.BlockHash, false);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<PublishedExternalBlockItemEntity> GetCollection()
        {
            var col = _database.GetCollection<PublishedExternalBlockItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(OmniHash rootHash, OmniHash blockHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);
                var blockHashEntity = OmniHashEntity.Import(blockHash);

                var col = this.GetCollection();
                return col.Exists(n => n.BlockHash == blockHashEntity && n.RootHash == rootHashEntity);
            }
        }

        public IEnumerable<PublishedExternalBlockItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<PublishedExternalBlockItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<PublishedExternalBlockItem> Find(OmniHash rootHash, OmniHash blockHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);
                var blockHashEntity = OmniHashEntity.Import(blockHash);

                var col = this.GetCollection();
                return col.Find(n => n.BlockHash == blockHashEntity && n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public void Upsert(PublishedExternalBlockItem item)
        {
            this.UpsertBulk(new[] { item });
        }

        public void UpsertBulk(IEnumerable<PublishedExternalBlockItem> items)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();

                _database.BeginTrans();

                foreach (var item in items)
                {
                    var itemEntity = PublishedExternalBlockItemEntity.Import(item);

                    col.DeleteMany(n =>
                        n.BlockHash == itemEntity.BlockHash
                        && n.FilePath == itemEntity.FilePath
                        && n.RootHash == itemEntity.RootHash
                        && n.Order == itemEntity.Order
                        && n.Offset == itemEntity.Offset
                        && n.Count == itemEntity.Count);
                    col.Insert(itemEntity);
                }

                _database.Commit();
            }
        }

        public void Delete(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                col.DeleteMany(n => n.RootHash == rootHashEntity);
            }
        }
    }
}
