using System.Data;
using System.Data.SQLite;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.RocketPack;
using Omnius.Core.Serialization;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Omnius.Axus.Interactors.Internal.Repositories;

internal sealed class CachedSeedBoxRepository
{
    private readonly string _databasePath;
    private readonly IBytesPool _bytesPool;

    private readonly AsyncLock _asyncLock = new();

    public CachedSeedBoxRepository(string dirPath, IBytesPool bytesPool)
    {
        DirectoryHelper.CreateDirectory(dirPath);
        _databasePath = Path.Combine(dirPath, "sqlite.db");
        _bytesPool = bytesPool;
    }

    private async ValueTask<SQLiteConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var sqlConnectionStringBuilder = new SQLiteConnectionStringBuilder
        {
            DataSource = _databasePath,
            ForeignKeys = true,
        };
        var connection = new SQLiteConnection(sqlConnectionStringBuilder.ToString());
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await this.GetConnectionAsync(cancellationToken);

            var query =
    @"
CREATE TABLE IF NOT EXISTS boxes (
    signature TEXT NOT NULL PRIMARY KEY,
    created_time INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS seeds (
    self_hash TEXT NOT NULL PRIMARY KEY,
    signature TEXT NOT NULL,
    name TEXT NOT NULL,
    size INTEGER NOT NULL,
    created_time INTEGER NOT NULL,
    value BLOB NOT NULL
);
CREATE INDEX IF NOT EXISTS index_signature_and_created_time_for_boxes ON boxes (signature, created_time);
CREATE INDEX IF NOT EXISTS index_signature_for_seeds ON seeds (signature);
CREATE INDEX IF NOT EXISTS index_size_for_seeds ON seeds (size);
";
            await connection.ExecuteNonQueryAsync(query, cancellationToken);
        }
    }

    public async ValueTask UpsertAsync(CachedSeedBox box, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await this.GetConnectionAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            {
                var query =
$@"
DELETE FROM boxes WHERE signature = @Signature;
INSERT INTO boxes (signature, created_time)
    VALUES (@Signature, @CreatedTime);
DELETE FROM seeds WHERE signature = @Signature;
";
                var parameters = new (string, object)[] {
                    ("@Signature", box.Signature.ToString()),
                    ("@CreatedTime", box.CreatedTime.Seconds)
                };

                await transaction.ExecuteNonQueryAsync(query, parameters, cancellationToken);
            }

            foreach (var s in box.ToCachedSeeds())
            {
                var query =
$@"
INSERT OR IGNORE INTO seeds (self_hash, signature, name, size, created_time, value)
    VALUES (@SelfHash, @Signature, @Name, @Size, @CreatedTime, @Value);
";
                using var valueBytes = RocketMessage.ToBytes(s.Value);
                var parameters = new (string, object)[] {
                    ("@SelfHash", s.SelfHash.ToString(ConvertStringType.Base16)),
                    ("@Signature", s.Signature.ToString()),
                    ("@Name", s.Value.Name),
                    ("@Size", s.Value.Size),
                    ("@CreatedTime", s.Value.CreatedTime.Seconds),
                    ("@Value", valueBytes),
                };

                await transaction.ExecuteNonQueryAsync(query, parameters, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async ValueTask<IEnumerable<(OmniSignature Signature, Timestamp64 CreatedTime)>> GetKeysAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await this.GetConnectionAsync(cancellationToken);
            var compiler = new SqliteCompiler();
            using var db = new QueryFactory(connection, compiler);

            var rows = await db.Query("profiles")
                .Select("signature", "created_time")
                .GetAsync();

            var results = rows
                .Select(n => (n.signature, n.created_time))
                .OfType<(string, long)>()
                .Select(n => (OmniSignature.Parse(n.Item1), new Timestamp64(n.Item2)));

            return results;
        }
    }

    // TODO: FindSeedsの実装が必要

    public async ValueTask ShrinkAsync(IEnumerable<OmniSignature> excludedSignatures, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await this.GetConnectionAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            {
                var query =
@"
CREATE TEMP TABLE excluded_signatures (
    signature TEXT NOT NULL PRIMARY KEY
);
";
                await transaction.ExecuteNonQueryAsync(query, cancellationToken);
            }

            {
                var compiler = new SqliteCompiler();
                using var db = new QueryFactory(connection, compiler);

                foreach (var chunkedSignatures in excludedSignatures.Chunk(500))
                {
                    var columns = new[] { "signature" };
                    var values = chunkedSignatures.Select(signature => new object[] { signature.ToString() });
                    await db.Query("excluded_signatures")
                        .InsertAsync(columns, values, transaction, null, cancellationToken);
                }
            }

            {
                var query =
@"
DELETE FROM boxes
    WHERE (signature) NOT IN (SELECT (signature) FROM excluded_signatures);
";
                await transaction.ExecuteNonQueryAsync(query, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }
}
