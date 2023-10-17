using System.Runtime.CompilerServices;
using System.Text;
using Omnius.Axus.Core.Engine.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.RocketPack;
using Omnius.Core.Serialization;
using Omnius.Core.Sql;
using SqlKata.Compilers;
using SqlKata.Execution;
using Omnius.Axus.Core.Engine.Repositories.Helpers;
using Omnius.Axus.Core.Engine.Repositories.Models;

namespace Omnius.Axus.Core.Engine.Repositories;

internal sealed partial class FileSubscriberStorageRepository : AsyncDisposableBase
{
    private readonly SQLiteConnectionBuilder _connectionBuilder;
    private readonly IBytesPool _bytesPool;

    private const ConvertBaseType _convertBaseType = ConvertBaseType.Base64;

    public FileSubscriberStorageRepository(string dirPath, IBytesPool bytesPool)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _connectionBuilder = new SQLiteConnectionBuilder(Path.Combine(dirPath, "sqlite.db"));
        _bytesPool = bytesPool;

        this.FileItems = new FileSubscribedItemRepository(_connectionBuilder, _bytesPool);
        this.BlockItems = new BlockSubscribedItemRepository(_connectionBuilder, _bytesPool);
    }

    protected override async ValueTask OnDisposeAsync()
    {
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
        private readonly SQLiteConnectionBuilder _connectionBuilder;
        private readonly IBytesPool _bytesPool;

        private readonly AsyncLock _asyncLock = new();

        public FileSubscribedItemRepository(SQLiteConnectionBuilder connectionBuilder, IBytesPool bytesPool)
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
CREATE TABLE IF NOT EXISTS files (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    root_hash TEXT NOT NULL,
    current_depth INTEGER NOT NULL,
    total_block_count INTEGER NOT NULL,
    downloaded_block_count INTEGER NOT NULL,
    state INTEGER NOT NULL,
    property TEXT,
    created_time INTEGER NOT NULL,
    updated_time INTEGER NOT NULL,
    UNIQUE (root_hash)
);
CREATE INDEX IF NOT EXISTS index_state_for_files ON files (state);
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
    FROM files
    WHERE root_hash = @root_hash
    LIMIT 1;
";
                var parameters = new (string, object?)[]
                {
                    ("@root_hash", rootHash.ToString(_convertBaseType))
                };

                var result = await connection.ExecuteScalarAsync(query, parameters, cancellationToken);
                return (long)result! == 1;
            }
        }

        public async IAsyncEnumerable<FileSubscribedItem> GetItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    var rows = await db.Query("files")
                        .Select("root_hash", "current_depth", "total_block_count", "downloaded_block_count", "state", "property", "created_time", "updated_time")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return new FileSubscribedItem
                        {
                            RootHash = OmniHash.Parse((string)row.root_hash),
                            Status = new FileSubscribedItemStatus
                            {
                                CurrentDepth = (int)row.current_depth,
                                TotalBlockCount = (int)row.total_block_count,
                                DownloadedBlockCount = (int)row.downloaded_block_count,
                                State = (SubscribedFileState)row.state,
                            },
                            Property = row.property is null ? null : AttachedProperty.Create(row.property),
                            CreatedTime = Timestamp64.FromSeconds((long)row.created_time).ToDateTime(),
                            UpdatedTime = Timestamp64.FromSeconds((long)row.updated_time).ToDateTime(),
                        };
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async ValueTask<FileSubscribedItem?> GetItemAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
                var compiler = new SqliteCompiler();
                using var db = new QueryFactory(connection, compiler);

                var rows = await db.Query("files")
                    .Select("root_hash", "current_depth", "total_block_count", "downloaded_block_count", "state", "property", "created_time", "updated_time")
                    .Where("root_hash", "=", rootHash.ToString(_convertBaseType))
                    .Limit(1)
                    .GetAsync(cancellationToken: cancellationToken);
                if (!rows.Any()) return null;

                var row = rows.First();

                return new FileSubscribedItem
                {
                    RootHash = OmniHash.Parse((string)row.root_hash),
                    Status = new FileSubscribedItemStatus
                    {
                        CurrentDepth = (int)row.current_depth,
                        TotalBlockCount = (int)row.total_block_count,
                        DownloadedBlockCount = (int)row.downloaded_block_count,
                        State = (SubscribedFileState)row.state,
                    },
                    Property = row.property is null ? null : AttachedProperty.Create(row.property),
                    CreatedTime = Timestamp64.FromSeconds((long)row.created_time).ToDateTime(),
                    UpdatedTime = Timestamp64.FromSeconds((long)row.updated_time).ToDateTime(),
                };
            }
        }

        public async IAsyncEnumerable<FileSubscribedItem> GetItemsAsync(SubscribedFileState state, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    var rows = await db.Query("files")
                        .Select("root_hash", "current_depth", "total_block_count", "downloaded_block_count", "state", "property", "created_time", "updated_time")
                        .Where("state", "=", (int)state)
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return new FileSubscribedItem
                        {
                            RootHash = OmniHash.Parse((string)row.root_hash),
                            Status = new FileSubscribedItemStatus
                            {
                                CurrentDepth = (int)row.current_depth,
                                TotalBlockCount = (int)row.total_block_count,
                                DownloadedBlockCount = (int)row.downloaded_block_count,
                                State = (SubscribedFileState)row.state,
                            },
                            Property = row.property is null ? null : AttachedProperty.Create(row.property),
                            CreatedTime = Timestamp64.FromSeconds((long)row.created_time).ToDateTime(),
                            UpdatedTime = Timestamp64.FromSeconds((long)row.updated_time).ToDateTime(),
                        };
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
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
                    var rows = await db.Query("files")
                        .Select("root_hash")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return OmniHash.Parse((string)row.root_hash);
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async ValueTask UpsertAsync(FileSubscribedItem item, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
INSERT INTO files (root_hash, current_depth, total_block_count, downloaded_block_count, state, property, created_time, updated_time)
    VALUES (@root_hash, @current_depth, @total_block_count, @downloaded_block_count, @state, @property, @created_time, @updated_time)
    ON CONFLICT(root_hash) DO UPDATE SET
        current_depth = @current_depth,
        total_block_count = @total_block_count,
        downloaded_block_count = @downloaded_block_count,
        state = @state,
        property = @property,
        created_time = @created_time,
        updated_time = @updated_time;
";
                var parameters = new (string, object?)[]
                {
                    ("@root_hash", item.RootHash.ToString(_convertBaseType)),
                    ("@current_depth", item.Status.CurrentDepth),
                    ("@total_block_count", item.Status.TotalBlockCount),
                    ("@downloaded_block_count", item.Status.DownloadedBlockCount),
                    ("@state", (int)item.Status.State),
                    ("@property", item.Property?.Value),
                    ("@created_time", Timestamp64.FromDateTime(item.CreatedTime).Seconds),
                    ("@updated_time", Timestamp64.FromDateTime(item.UpdatedTime).Seconds)
                };

                var result = await connection.ExecuteNonQueryAsync(query, parameters, cancellationToken);
            }
        }

        public async ValueTask DeleteAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
DELETE
    FROM files
    WHERE root_hash = @root_hash;
";
                var parameters = new (string, object?)[]
                {
                    ($"@root_hash", rootHash.ToString(_convertBaseType)),
                };

                var result = await connection.ExecuteNonQueryAsync(query, parameters, cancellationToken);
            }
        }
    }

    public sealed class BlockSubscribedItemRepository
    {
        private readonly SQLiteConnectionBuilder _connectionBuilder;
        private readonly IBytesPool _bytesPool;

        private readonly AsyncLock _asyncLock = new();

        public BlockSubscribedItemRepository(SQLiteConnectionBuilder connectionBuilder, IBytesPool bytesPool)
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
CREATE TABLE IF NOT EXISTS blocks (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    root_hash TEXT NOT NULL,
    block_hash TEXT NOT NULL,
    depth INTEGER NOT NULL,
    `index` INTEGER NOT NULL,
    is_downloaded INTEGER NOT NULL,
    UNIQUE (root_hash, block_hash, depth, `index`)
);
CREATE INDEX IF NOT EXISTS index_root_hash_depth_index_for_blocks ON blocks (root_hash, depth ASC, `index` ASC, is_downloaded);
";
                await connection.ExecuteNonQueryAsync(query, cancellationToken);
            }
        }

        public async ValueTask<bool> ExistsAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
SELECT COUNT(1)
    FROM blocks
    WHERE root_hash = @root_hash AND block_hash = @block_hash
    LIMIT 1;
";
                var parameters = new (string, object?)[]
                {
                    ($"@root_hash", rootHash.ToString(_convertBaseType)),
                    ($"@block_hash", blockHash.ToString(_convertBaseType)),
                };

                var result = await connection.ExecuteScalarAsync(query, parameters, cancellationToken);
                return (long)result! == 1;
            }
        }

        public async IAsyncEnumerable<BlockSubscribedItem> GetItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    var rows = await db.Query("blocks")
                        .Select("root_hash", "block_hash", "depth", "index", "is_downloaded")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return new BlockSubscribedItem
                        {
                            RootHash = OmniHash.Parse((string)row.root_hash),
                            BlockHash = OmniHash.Parse((string)row.block_hash),
                            Depth = (int)row.depth,
                            Index = (int)row.index,
                            IsDownloaded = (bool)(row.is_downloaded == 1),
                        };
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async IAsyncEnumerable<BlockSubscribedItem> GetItemsAsync(OmniHash rootHash, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    var rows = await db.Query("blocks")
                        .Select("root_hash", "block_hash", "depth", "index", "is_downloaded")
                        .Where("root_hash", "=", rootHash.ToString(_convertBaseType))
                        .OrderBy("depth", "index")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return new BlockSubscribedItem
                        {
                            RootHash = OmniHash.Parse((string)row.root_hash),
                            BlockHash = OmniHash.Parse((string)row.block_hash),
                            Depth = (int)row.depth,
                            Index = (int)row.index,
                            IsDownloaded = (bool)(row.is_downloaded == 1),
                        };
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async IAsyncEnumerable<BlockSubscribedItem> GetItemsAsync(OmniHash rootHash, OmniHash blockHash, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    var rows = await db.Query("blocks")
                        .Select("root_hash", "block_hash", "depth", "index", "is_downloaded")
                        .Where("root_hash", "=", rootHash.ToString(_convertBaseType))
                        .Where("block_hash", "=", blockHash.ToString(_convertBaseType))
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return new BlockSubscribedItem
                        {
                            RootHash = OmniHash.Parse((string)row.root_hash),
                            BlockHash = OmniHash.Parse((string)row.block_hash),
                            Depth = (int)row.depth,
                            Index = (int)row.index,
                            IsDownloaded = (bool)(row.is_downloaded == 1),
                        };
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async ValueTask<BlockSubscribedItem?> GetItemAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
                var compiler = new SqliteCompiler();
                using var db = new QueryFactory(connection, compiler);

                var rows = await db.Query("blocks")
                    .Select("root_hash", "block_hash", "depth", "index", "is_downloaded")
                    .Where("root_hash", "=", rootHash.ToString(_convertBaseType))
                    .Where("block_hash", "=", blockHash.ToString(_convertBaseType))
                    .Limit(1)
                    .GetAsync(cancellationToken: cancellationToken);
                if (!rows.Any()) return null;

                var row = rows.First();

                return new BlockSubscribedItem
                {
                    RootHash = OmniHash.Parse((string)row.root_hash),
                    BlockHash = OmniHash.Parse((string)row.block_hash),
                    Depth = (int)row.depth,
                    Index = (int)row.index,
                    IsDownloaded = (bool)(row.is_downloaded == 1),
                };
            }
        }

        public async IAsyncEnumerable<OmniHash> GetBlockHashesAsync(OmniHash rootHash, int depth, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    var rows = await db.Query("blocks")
                        .Select("block_hash")
                        .Where("root_hash", "=", rootHash.ToString(_convertBaseType))
                        .Where("depth", "=", depth)
                        .OrderBy("index")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return OmniHash.Parse((string)row.block_hash);
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async IAsyncEnumerable<OmniHash> GetBlockHashesAsync(OmniHash rootHash, bool isDownloaded, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    var rows = await db.Query("blocks")
                        .Select("block_hash")
                        .Where("root_hash", "=", rootHash.ToString(_convertBaseType))
                        .Where("is_downloaded", "=", isDownloaded ? 1 : 0)
                        .OrderBy("depth", "index")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return OmniHash.Parse((string)row.block_hash);
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async ValueTask UpsertBulkAsync(IEnumerable<BlockSubscribedItem> items, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                foreach (var chunkedItems in items.Chunk(500))
                {
                    var queries = new StringBuilder();
                    var parameters = new List<(string, object?)>();

                    foreach (var (i, item) in chunkedItems.Select((n, i) => (i, n)))
                    {
                        var q =
$@"
INSERT INTO blocks (root_hash, block_hash, depth, `index`, is_downloaded)
    VALUES (@root_hash_{i}, @block_hash_{i}, @depth_{i}, @index_{i}, @is_downloaded_{i})
    ON CONFLICT (root_hash, block_hash, depth, `index`) DO UPDATE SET
        is_downloaded = @is_downloaded_{i};
";
                        queries.Append(q);

                        var ps = new (string, object?)[]
                        {
                            ($"@root_hash_{i}", item.RootHash.ToString(_convertBaseType)),
                            ($"@block_hash_{i}", item.BlockHash.ToString(_convertBaseType)),
                            ($"@depth_{i}", item.Depth),
                            ($"@index_{i}", item.Index),
                            ($"@is_downloaded_{i}", item.IsDownloaded ? 1 : 0),
                        };
                        parameters.AddRange(ps);
                    }

                    await transaction.ExecuteNonQueryAsync(queries.ToString(), parameters, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
        }

        public async ValueTask DeleteAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
DELETE
    FROM blocks
    WHERE root_hash = @root_hash;
";
                var parameters = new (string, object?)[]
                {
                    ($"@root_hash", rootHash.ToString(_convertBaseType)),
                };

                var result = await connection.ExecuteNonQueryAsync(query, parameters, cancellationToken);
            }
        }
    }
}