using System.Runtime.CompilerServices;
using Omnius.Axus.Core.Engine.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.RocketPack;
using Omnius.Core.Sql;
using SqlKata.Compilers;
using SqlKata.Execution;
using Omnius.Axus.Core.Engine.Repositories.Helpers;
using Omnius.Axus.Core.Engine.Repositories.Models;

namespace Omnius.Axus.Core.Engine.Repositories;

internal sealed partial class ShoutSubscriberStorageRepository : AsyncDisposableBase
{
    private readonly SQLiteConnectionBuilder _connectionBuilder;
    private readonly IBytesPool _bytesPool;

    public ShoutSubscriberStorageRepository(string dirPath, IBytesPool bytesPool)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _connectionBuilder = new SQLiteConnectionBuilder(Path.Combine(dirPath, "sqlite.db"));
        _bytesPool = bytesPool;

        this.ShoutItems = new ShoutSubscribedItemRepository(_connectionBuilder, _bytesPool);
    }

    protected override async ValueTask OnDisposeAsync()
    {
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.ShoutItems.MigrateAsync(cancellationToken);
    }

    public ShoutSubscribedItemRepository ShoutItems { get; }

    public sealed class ShoutSubscribedItemRepository
    {
        private readonly SQLiteConnectionBuilder _connectionBuilder;
        private readonly IBytesPool _bytesPool;

        private readonly AsyncLock _asyncLock = new();

        public ShoutSubscribedItemRepository(SQLiteConnectionBuilder connectionBuilder, IBytesPool bytesPool)
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
CREATE TABLE IF NOT EXISTS items (
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    signature TEXT NOT NULL,
    channel TEXT NOT NULL,
    property TEXT,
    shout_created_time INTEGER NOT NULL,
    created_time INTEGER NOT NULL,
    updated_time INTEGER NOT NULL,
    UNIQUE (signature, channel)
);
";
                await connection.ExecuteNonQueryAsync(query, cancellationToken);
            }
        }

        public async ValueTask<bool> ExistsAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
SELECT COUNT(1)
    FROM items
    WHERE signature = @signature AND channel = @channel
    LIMIT 1;
";
                var parameters = new (string, object?)[]
                {
                    ("@signature", signature.ToString()),
                    ("@channel", channel),
                };

                var result = await connection.ExecuteScalarAsync(query, parameters, cancellationToken);
                return (long)result! == 1;
            }
        }

        public async IAsyncEnumerable<ShoutSubscribedItem> GetItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                    var rows = await db.Query("items")
                        .Select("signature", "channel", "property", "shout_created_time", "created_time", "updated_time")
                        .Offset(offset)
                        .Limit(limit)
                        .GetAsync(cancellationToken: cancellationToken);
                    if (!rows.Any()) yield break;

                    foreach (var row in rows)
                    {
                        yield return new ShoutSubscribedItem
                        {
                            Signature = OmniSignature.Parse((string)row.signature),
                            Channel = (string)row.channel,
                            Property = row.property is null ? null : AttachedProperty.Create((string)row.property),
                            ShoutCreatedTime = Timestamp64.FromSeconds((long)row.shout_created_time).ToDateTime(),
                            CreatedTime = Timestamp64.FromSeconds((long)row.created_time).ToDateTime(),
                            UpdatedTime = Timestamp64.FromSeconds((long)row.updated_time).ToDateTime(),
                        };
                    }

                    offset = limit;
                    limit += ChunkSize;
                }
            }
        }

        public async ValueTask<ShoutSubscribedItem?> GetItemAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
                var compiler = new SqliteCompiler();
                using var db = new QueryFactory(connection, compiler);

                var rows = await db.Query("items")
                    .Select("signature", "channel", "property", "shout_created_time", "created_time", "updated_time")
                    .Where("signature", signature.ToString())
                    .Where("channel", channel)
                    .Limit(1)
                    .GetAsync(cancellationToken: cancellationToken);
                if (!rows.Any()) return null;

                var row = rows.First();

                return new ShoutSubscribedItem
                {
                    Signature = OmniSignature.Parse((string)row.signature),
                    Channel = (string)row.channel,
                    Property = row.property is null ? null : AttachedProperty.Create((string)row.property),
                    ShoutCreatedTime = Timestamp64.FromSeconds((long)row.shout_created_time).ToDateTime(),
                    CreatedTime = Timestamp64.FromSeconds((long)row.created_time).ToDateTime(),
                    UpdatedTime = Timestamp64.FromSeconds((long)row.updated_time).ToDateTime(),
                };
            }
        }

        public async ValueTask UpsertAsync(ShoutSubscribedItem item, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
INSERT INTO items (signature, channel, property, shout_created_time, created_time, updated_time)
    VALUES (@signature, @channel, @property, @shout_created_time, @created_time, @updated_time)
    ON CONFLICT(signature, channel) DO UPDATE SET
        property = @property,
        shout_created_time = @shout_created_time,
        created_time = @created_time,
        updated_time = @updated_time;
";
                var parameters = new (string, object?)[]
                {
                    ("@signature", item.Signature.ToString()),
                    ("@channel", item.Channel),
                    ("@property", item.Property?.Value),
                    ("@shout_created_time", Timestamp64.FromDateTime(item.ShoutCreatedTime).Seconds),
                    ("@created_time", Timestamp64.FromDateTime(item.CreatedTime).Seconds),
                    ("@updated_time", Timestamp64.FromDateTime(item.UpdatedTime).Seconds)
                };

                var result = await connection.ExecuteNonQueryAsync(query, parameters, cancellationToken);
            }
        }

        public async ValueTask DeleteAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

                var query =
$@"
DELETE
    FROM items
    WHERE signature = @signature AND channel = @channel;
";
                var parameters = new (string, object?)[]
                {
                    ("@signature", signature.ToString()),
                    ("@channel", channel),
                };

                var result = await connection.ExecuteNonQueryAsync(query, parameters, cancellationToken);
            }
        }
    }
}
