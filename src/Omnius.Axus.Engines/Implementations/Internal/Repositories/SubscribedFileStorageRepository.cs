using LiteDB;
using Omnius.Axus.Engines.Internal.Entities;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Engines.Internal.Repositories;

internal sealed partial class SubscribedFileStorageRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public SubscribedFileStorageRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.FileItems = new SubscribedFileItemRepository(_database);
        this.BlockItems = new SubscribedBlockItemRepository(_database);
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

    public SubscribedFileItemRepository FileItems { get; }
    public SubscribedBlockItemRepository BlockItems { get; }

    public sealed class SubscribedFileItemRepository
    {
        private const string CollectionName = "subscribed_file_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public SubscribedFileItemRepository(LiteDatabase database)
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

        private ILiteCollection<SubscribedFileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<SubscribedFileItemEntity>(CollectionName);
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

        public IEnumerable<SubscribedFileItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public SubscribedFileItem? FindOne(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.RootHash == rootHashEntity)?.Export();
            }
        }

        public void Upsert(SubscribedFileItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = SubscribedFileItemEntity.Import(item);

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

    public sealed class SubscribedBlockItemRepository
    {
        private const string CollectionName = "subscribed_block_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public SubscribedBlockItemRepository(LiteDatabase database)
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

        private ILiteCollection<SubscribedBlockItemEntity> GetCollection()
        {
            var col = _database.GetCollection<SubscribedBlockItemEntity>(CollectionName);
            return col;
        }

        public IEnumerable<SubscribedBlockItem> FindAll()
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

        public IEnumerable<SubscribedBlockItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<SubscribedBlockItem> Find(OmniHash rootHash, OmniHash blockHash)
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

        public void Upsert(SubscribedBlockItem item)
        {
            this.UpsertBulk(new[] { item });
        }

        public void UpsertBulk(IEnumerable<SubscribedBlockItem> items)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();

                _database.BeginTrans();

                foreach (var item in items)
                {
                    var itemEntity = SubscribedBlockItemEntity.Import(item);

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
