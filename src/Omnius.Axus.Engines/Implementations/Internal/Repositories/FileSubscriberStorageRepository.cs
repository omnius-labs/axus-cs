using LiteDB;
using Omnius.Axus.Engines.Internal.Entities;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Engines.Internal.Repositories;

internal sealed partial class FileSubscriberStorageRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public FileSubscriberStorageRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.FileItems = new FileSubscribedItemRepository(_database);
        this.BlockItems = new BlockSubscribedItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.FileItems.MigrateAsync(cancellationToken);
        await this.BlockItems.MigrateAsync(cancellationToken);
    }

    public FileSubscribedItemRepository FileItems { get; }
    public BlockSubscribedItemRepository BlockItems { get; }

    public sealed class FileSubscribedItemRepository
    {
        private const string CollectionName = "subscribed_file_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public FileSubscribedItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.RootHash, true);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<FileSubscribedItemEntity> GetCollection()
        {
            var col = _database.GetCollection<FileSubscribedItemEntity>(CollectionName);
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

        public IEnumerable<FileSubscribedItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<FileSubscribedItem> Find(string zone)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.Find(n => n.Authors!.Contains(zone)).Select(n => n.Export()).ToArray();
            }
        }

        public FileSubscribedItem? FindOne(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.RootHash == rootHashEntity)?.Export();
            }
        }

        public void Upsert(FileSubscribedItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = FileSubscribedItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.RootHash == itemEntity.RootHash);
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
                col.DeleteMany(n => n.RootHash == rootHashEntity);
            }
        }
    }

    public sealed class BlockSubscribedItemRepository
    {
        private const string CollectionName = "subscribed_block_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public BlockSubscribedItemRepository(LiteDatabase database)
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

        private ILiteCollection<BlockSubscribedItemEntity> GetCollection()
        {
            var col = _database.GetCollection<BlockSubscribedItemEntity>(CollectionName);
            return col;
        }

        public IEnumerable<BlockSubscribedItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
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

        public IEnumerable<BlockSubscribedItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<BlockSubscribedItem> Find(OmniHash rootHash, OmniHash blockHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);
                var blockHashEntity = OmniHashEntity.Import(blockHash);

                var col = this.GetCollection();
                return col.Find(n => n.BlockHash == blockHashEntity && n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<OmniHash> FindBlockHashes(OmniHash rootHash, int depth)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity && n.Depth == depth)
                    .OrderBy(n => n.Order)
                    .Select(n => n.BlockHash!.Export())
                    .ToArray();
            }
        }

        public void Upsert(BlockSubscribedItem item)
        {
            this.UpsertBulk(new[] { item });
        }

        public void UpsertBulk(IEnumerable<BlockSubscribedItem> items)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();

                _database.BeginTrans();

                foreach (var item in items)
                {
                    var itemEntity = BlockSubscribedItemEntity.Import(item);

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
}
