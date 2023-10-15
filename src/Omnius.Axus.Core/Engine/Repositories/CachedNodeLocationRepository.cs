using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
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

internal sealed class CachedNodeLocationRepository : AsyncDisposableBase
{
    private readonly SQLiteConnectionBuilder _connectionBuilder;
    private readonly IBytesPool _bytesPool;

    private readonly AsyncLock _asyncLock = new();

    public CachedNodeLocationRepository(string dirPath, IBytesPool bytesPool)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _connectionBuilder = new SQLiteConnectionBuilder(Path.Combine(dirPath, "sqlite.db"));
        _bytesPool = bytesPool;
    }

    protected override async ValueTask OnDisposeAsync()
    {
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

            var query =
@"
CREATE TABLE IF NOT EXISTS node_locations (
    hash TEXT NOT NULL PRIMARY KEY,
    value BLOB NOT NULL,
    created_time INTEGER NOT NULL,
    updated_time INTEGER NOT NULL
);
";
            await connection.ExecuteNonQueryAsync(query, cancellationToken);
        }
    }

    public async IAsyncEnumerable<CachedNodeLocation> GetNodeLocationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                var rows = await db.Query("node_locations")
                    .Select("value", "created_time", "updated_time")
                    .Offset(offset)
                    .Limit(limit)
                    .GetAsync(cancellationToken: cancellationToken);
                if (!rows.Any()) yield break;

                foreach (var row in rows)
                {
                    var nodeLocation = RocketMessageConverter.FromBytes<NodeLocation>(row.value);
                    yield return new CachedNodeLocation
                    {
                        Value = nodeLocation,
                        CreatedTime = Timestamp64.FromSeconds((long)row.created_time).ToDateTime(),
                        UpdatedTime = Timestamp64.FromSeconds((long)row.updated_time).ToDateTime()
                    };
                }

                offset = limit;
                limit += ChunkSize;
            }
        }
    }

    public async ValueTask InsertAsync(NodeLocation nodeLocation, CancellationToken cancellationToken = default)
    {
        await this.InsertBulkAsync(new[] { nodeLocation });
    }

    public async ValueTask InsertBulkAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            var now = Timestamp64.FromDateTime(DateTime.UtcNow).Seconds;

            foreach (var chunk in nodeLocations.Chunk(500))
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.AppendLine("INSERT OR IGNORE INTO node_locations (hash, value, created_time, updated_time) VALUES");
                var parameters = new List<(string, object?)>();
                parameters.Add(($"@created_time", now));
                parameters.Add(($"@updated_time", now));

                foreach (var (_, current, next, index) in chunk.WithContext())
                {
                    queryBuilder.AppendLine($"(@hash_{index}, @value_{index}, @created_time, @updated_time)");
                    if (next is not null) queryBuilder.AppendLine(",");

                    using var valueBytes = RocketMessageConverter.ToBytes(current);
                    var hash = OmniHash.Create(OmniHashAlgorithmType.Sha2_256, valueBytes.Memory.Span);

                    parameters.Add(($"@hash_{index}", hash.ToString()));
                    parameters.Add(($"@value_{index}", valueBytes.Memory.ToArray()));
                }

                await transaction.ExecuteNonQueryAsync(queryBuilder.ToString(), parameters, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async ValueTask UpsertAsync(NodeLocation nodeLocation, CancellationToken cancellationToken = default)
    {
        await this.UpsertBulkAsync(new[] { nodeLocation });
    }

    public async ValueTask UpsertBulkAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            var now = Timestamp64.FromDateTime(DateTime.UtcNow).Seconds;

            foreach (var chunk in nodeLocations.Chunk(500))
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.AppendLine("INSERT INTO node_locations (hash, value, created_time, updated_time) VALUES");
                var parameters = new List<(string, object?)>();
                parameters.Add(($"@created_time", now));
                parameters.Add(($"@updated_time", now));

                foreach (var (_, current, next, index) in chunk.WithContext())
                {
                    queryBuilder.AppendLine($"(@hash_{index}, @value_{index}, @created_time, @updated_time)");
                    if (next is not null) queryBuilder.AppendLine(",");

                    using var valueBytes = RocketMessageConverter.ToBytes(current);
                    var hash = OmniHash.Create(OmniHashAlgorithmType.Sha2_256, valueBytes.Memory.Span);

                    parameters.Add(($"@hash_{index}", hash.ToString()));
                    parameters.Add(($"@value_{index}", valueBytes.Memory.ToArray()));
                }

                queryBuilder.AppendLine("ON CONFLICT(hash) DO UPDATE SET updated_time = @updated_time");

                await transaction.ExecuteNonQueryAsync(queryBuilder.ToString(), parameters, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async ValueTask ShrinkAsync(int capacity, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);

        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await _connectionBuilder.CreateAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            {
                var query =
@"
CREATE TEMP TABLE tmp (
    hash TEXT NOT NULL PRIMARY KEY
);
";
                await transaction.ExecuteNonQueryAsync(query, cancellationToken);
            }

            {
                var query =
@$"
INSERT INTO tmp (hash)
    SELECT hash
        FROM node_locations
        ORDER BY updated_time DESC
        LIMIT {capacity}
";
                await transaction.ExecuteNonQueryAsync(query, cancellationToken);
            }

            {
                var query =
@"
DELETE FROM node_locations
    WHERE (hash) NOT IN (SELECT (hash) FROM tmp);
";
                await transaction.ExecuteNonQueryAsync(query, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async ValueTask<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await _connectionBuilder.CreateAsync(cancellationToken);

            var query = "SELECT COUNT(1) FROM node_locations";
            var res = await connection.ExecuteScalarAsync(query, cancellationToken);
            return (long)res!;
        }
    }
}
