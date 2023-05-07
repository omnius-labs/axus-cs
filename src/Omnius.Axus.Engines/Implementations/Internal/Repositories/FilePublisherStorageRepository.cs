using LiteDB;
using Omnius.Axus.Engines.Internal.Entities;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Engines.Internal.Repositories;

internal sealed class FilePublisherStorageRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public FilePublisherStorageRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.FileItems = new FilePublishedItemRepository(_database);
        this.BlockInternalItems = new BlockPublishedInternalItemRepository(_database);
        this.BlockExternalItems = new BlockPublishedExternalItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.FileItems.MigrateAsync(cancellationToken);
    }

    public FilePublishedItemRepository FileItems { get; }
    public BlockPublishedInternalItemRepository BlockInternalItems { get; }
    public BlockPublishedExternalItemRepository BlockExternalItems { get; }

    public sealed class FilePublishedItemRepository
    {
        private const string CollectionName = "published_file_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public FilePublishedItemRepository(LiteDatabase database)
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

        private ILiteCollection<FilePublishedItemEntity> GetCollection()
        {
            var col = _database.GetCollection<FilePublishedItemEntity>(CollectionName);
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

        public IEnumerable<FilePublishedItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<FilePublishedItem> Find(string zone)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.Find(n => n.Zones!.Contains(zone)).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<FilePublishedItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public FilePublishedItem? FindOne(string filePath)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindOne(n => n.FilePath == filePath)?.Export();
            }
        }

        public FilePublishedItem? FindOne(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.RootHash == rootHashEntity && n.FilePath == null)?.Export();
            }
        }

        public void Upsert(FilePublishedItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = FilePublishedItemEntity.Import(item);

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

    public sealed class BlockPublishedInternalItemRepository
    {
        private const string CollectionName = "published_internal_block_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public BlockPublishedInternalItemRepository(LiteDatabase database)
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

        private ILiteCollection<BlockPublishedInternalItemEntity> GetCollection()
        {
            var col = _database.GetCollection<BlockPublishedInternalItemEntity>(CollectionName);
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

        public IEnumerable<BlockPublishedInternalItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<BlockPublishedInternalItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<BlockPublishedInternalItem> Find(OmniHash rootHash, OmniHash blockHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);
                var blockHashEntity = OmniHashEntity.Import(blockHash);

                var col = this.GetCollection();
                return col.Find(n => n.BlockHash == blockHashEntity && n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public void Upsert(BlockPublishedInternalItem item)
        {
            this.UpsertBulk(new[] { item });
        }

        public void UpsertBulk(IEnumerable<BlockPublishedInternalItem> items)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();

                _database.BeginTrans();

                foreach (var item in items)
                {
                    var itemEntity = BlockPublishedInternalItemEntity.Import(item);

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

    public sealed class BlockPublishedExternalItemRepository
    {
        private const string CollectionName = "published_external_block_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public BlockPublishedExternalItemRepository(LiteDatabase database)
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

        private ILiteCollection<BlockPublishedExternalItemEntity> GetCollection()
        {
            var col = _database.GetCollection<BlockPublishedExternalItemEntity>(CollectionName);
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

        public IEnumerable<BlockPublishedExternalItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<BlockPublishedExternalItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<BlockPublishedExternalItem> Find(OmniHash rootHash, OmniHash blockHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);
                var blockHashEntity = OmniHashEntity.Import(blockHash);

                var col = this.GetCollection();
                return col.Find(n => n.BlockHash == blockHashEntity && n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public void Upsert(BlockPublishedExternalItem item)
        {
            this.UpsertBulk(new[] { item });
        }

        public void UpsertBulk(IEnumerable<BlockPublishedExternalItem> items)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();

                _database.BeginTrans();

                foreach (var item in items)
                {
                    var itemEntity = BlockPublishedExternalItemEntity.Import(item);

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
