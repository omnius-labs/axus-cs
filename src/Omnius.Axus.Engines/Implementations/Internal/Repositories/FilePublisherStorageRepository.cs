using System.Buffers;
using System.Runtime.CompilerServices;
using Omnius.Axus.Engines.Internal;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.RocketPack;
using Omnius.Core.Serialization;
using Omnius.Core.Sql;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Omnius.Axus.Engines.Implementations.Internal.Repositories;

internal sealed class FilePublisherStorageRepository : AsyncDisposableBase
{
    private readonly SQLiteConnectionBuilder _connectionBuilder;
    private readonly IBytesPool _bytesPool;

    public FilePublisherStorageRepository(string dirPath, IBytesPool bytesPool)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _connectionBuilder = new SQLiteConnectionBuilder(Path.Combine(dirPath, "sqlite.db"));
        _bytesPool = bytesPool;

        this.FileItems = new FilePublishedItemRepository(_connectionBuilder, _bytesPool);
        this.BlockInternalItems = new BlockPublishedInternalItemRepository(_connectionBuilder, _bytesPool);
        this.BlockExternalItems = new BlockPublishedExternalItemRepository(_connectionBuilder, _bytesPool);
    }

    protected override async ValueTask OnDisposeAsync()
    {
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.FileItems.MigrateAsync(cancellationToken);
        await this.BlockInternalItems.MigrateAsync(cancellationToken);
        await this.BlockExternalItems.MigrateAsync(cancellationToken);
    }

    public FilePublishedItemRepository FileItems { get; }
    public BlockPublishedInternalItemRepository BlockInternalItems { get; }
    public BlockPublishedExternalItemRepository BlockExternalItems { get; }

    public sealed class FilePublishedItemRepository
    {
        private readonly SQLiteConnectionBuilder _connectionBuilder;
        private readonly IBytesPool _bytesPool;

        private readonly AsyncLock _asyncLock = new();

        public FilePublishedItemRepository(SQLiteConnectionBuilder connectionBuilder, IBytesPool bytesPool)
        {
            _connectionBuilder = connectionBuilder;
            _bytesPool = bytesPool;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
    @"
CREATE TABLE IF NOT EXISTS file_items (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    root_hash TEXT NOT NULL,
    file_path TEXT,
    max_block_size INTEGER NOT NULL,
    properties BLOB NOT NULL,
    created_time INTEGER NOT NULL,
    updated_time INTEGER NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS file_items_root_hash_index ON file_items(root_hash) WHERE file_path IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS file_items_file_path_index ON file_items(file_path) WHERE file_path IS NOT NULL;
";
                await connection.ExecuteNonQueryAsync(query, cancellationToken);
            }
        }

        public async ValueTask<bool> ExistsAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
SELECT COUNT(1)
    FROM file_items
    WHERE root_hash = '{SqliteQueryHelper.EscapeText(rootHash.ToString())}'
    LIMIT 1
;
";

                var result = await connection.ExecuteScalarAsync(query, cancellationToken);
                return (long)result! == 1;
            }
        }

        public async ValueTask<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
SELECT COUNT(1)
    FROM file_items
    WHERE file_path = '{SqliteQueryHelper.EscapeText(filePath)}'
    LIMIT 1
;
";

                var result = await connection.ExecuteScalarAsync(query, cancellationToken);
                return (long)result! == 1;
            }
        }

        public async IAsyncEnumerable<FilePublishedItem> GetItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
                var compiler = new SqliteCompiler();
                using var db = new QueryFactory(connection, compiler);

                const int ChunkSize = 5000;
                int offset = 0;
                int limit = ChunkSize;

                for (; ; )
                {
                    var rows = await db.Query("file_items")
                        .Select("root_hash", "file_path", "max_block_size", "properties", "created_time", "updated_time")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return new FilePublishedItem
                        {
                            RootHash = OmniHash.Parse(row.root_hash),
                            FilePath = row.file_path,
                            MaxBlockSize = row.max_block_size,
                            Properties = AttachedProperties.Import(new ReadOnlySequence<byte>(row.Properties), _bytesPool).Values,
                            CreatedTime = Timestamp64.FromSeconds(row.created_time).ToDateTime(),
                            UpdatedTime = Timestamp64.FromSeconds(row.updated_time).ToDateTime(),
                        };
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async ValueTask<FilePublishedItem?> GetItemAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
                var compiler = new SqliteCompiler();
                using var db = new QueryFactory(connection, compiler);

                var rows = await db.Query("file_items")
                    .Select("root_hash", "file_path", "max_block_size", "properties", "created_time", "updated_time")
                    .Where("file_path", "=", filePath)
                    .Limit(1)
                    .GetAsync(cancellationToken: cancellationToken);
                if (!rows.Any()) return null;

                var row = rows.First();

                return new FilePublishedItem
                {
                    RootHash = OmniHash.Parse(row.root_hash),
                    FilePath = row.file_path,
                    MaxBlockSize = row.max_block_size,
                    Properties = AttachedProperties.Import(new ReadOnlySequence<byte>(row.Properties), _bytesPool).Values,
                    CreatedTime = Timestamp64.FromSeconds(row.created_time).ToDateTime(),
                    UpdatedTime = Timestamp64.FromSeconds(row.updated_time).ToDateTime(),
                };
            }
        }

        public async ValueTask<FilePublishedItem?> GetItemAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
                var compiler = new SqliteCompiler();
                using var db = new QueryFactory(connection, compiler);

                var rows = await db.Query("file_items")
                    .Select("root_hash", "file_path", "max_block_size", "properties", "created_time", "updated_time")
                    .Where("root_hash", "=", rootHash.ToString(ConvertStringType.Base64))
                    .Limit(1)
                    .GetAsync(cancellationToken: cancellationToken);
                if (!rows.Any()) return null;

                var row = rows.First();

                return new FilePublishedItem
                {
                    RootHash = OmniHash.Parse(row.root_hash),
                    FilePath = row.file_path,
                    MaxBlockSize = row.max_block_size,
                    Properties = AttachedProperties.Import(new ReadOnlySequence<byte>(row.Properties), _bytesPool).Values,
                    CreatedTime = Timestamp64.FromSeconds(row.created_time).ToDateTime(),
                    UpdatedTime = Timestamp64.FromSeconds(row.updated_time).ToDateTime(),
                };
            }
        }

        public async IAsyncEnumerable<OmniHash> GetRootHashesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
                var compiler = new SqliteCompiler();
                using var db = new QueryFactory(connection, compiler);

                const int ChunkSize = 5000;
                int offset = 0;
                int limit = ChunkSize;

                for (; ; )
                {
                    var rows = await db.Query("file_items")
                        .Select("root_hash")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return OmniHash.Parse(row.root_hash);
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async ValueTask UpsertAsync(FilePublishedItem item, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask DeleteAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
        }
    }

    public sealed class BlockPublishedInternalItemRepository
    {
        private readonly SQLiteConnectionBuilder _connectionBuilder;
        private readonly IBytesPool _bytesPool;

        private readonly AsyncLock _asyncLock = new();

        public BlockPublishedInternalItemRepository(SQLiteConnectionBuilder connectionBuilder, IBytesPool bytesPool)
        {
            _connectionBuilder = connectionBuilder;
            _bytesPool = bytesPool;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
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
        private readonly SQLiteConnectionBuilder _connectionBuilder;
        private readonly IBytesPool _bytesPool;

        private readonly AsyncLock _asyncLock = new();

        public BlockPublishedExternalItemRepository(SQLiteConnectionBuilder connectionBuilder, IBytesPool bytesPool)
        {
            _connectionBuilder = connectionBuilder;
            _bytesPool = bytesPool;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
    @"
CREATE TABLE IF NOT EXISTS file_items (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    root_hash TEXT NOT NULL,
    file_path TEXT,
    max_block_size INTEGER NOT NULL,
    properties BLOB NOT NULL,
    created_time INTEGER NOT NULL,
    updated_time INTEGER NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS file_items_root_hash_index ON file_items(root_hash) WHERE file_path IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS file_items_file_path_index ON file_items(file_path) WHERE file_path IS NOT NULL;
";
                await connection.ExecuteNonQueryAsync(query, cancellationToken);
            }
        }

        public ValueTask<bool> ExistsAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
        }

        public async IAsyncEnumerable<BlockPublishedExternalItem> GetItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
        }

        public async IAsyncEnumerable<BlockPublishedExternalItem> GetItemsAsync(OmniHash rootHash, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
        }

        public async IAsyncEnumerable<BlockPublishedExternalItem> GetItemsAsync(OmniHash rootHash, OmniHash blockHash, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<BlockPublishedExternalItem> GetItemAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask UpsertAsync(BlockPublishedExternalItem item, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask UpsertBulkAsync(IEnumerable<BlockPublishedExternalItem> items, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask DeleteAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
        }
    }
}
