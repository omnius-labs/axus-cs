using System.Buffers;
using System.Data.SQLite;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;
using Omnius.Core.Serialization;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Omnius.Axus.Interactors.Internal.Repositories;

internal sealed class CachedNoteBoxRepository
{
    private readonly string _databasePath;
    private readonly IBytesPool _bytesPool;

    private static readonly Lazy<Base16> _base16 = new Lazy<Base16>(() => new Base16(ConvertStringCase.Lower));

    private readonly AsyncLock _asyncLock = new();

    public CachedNoteBoxRepository(string dirPath, IBytesPool bytesPool)
    {
        DirectoryHelper.CreateDirectory(dirPath);
        _databasePath = Path.Combine(dirPath, "sqlite.db");
        _bytesPool = bytesPool;
    }

    private async ValueTask<SQLiteConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var sqlConnectionStringBuilder = new SQLiteConnectionStringBuilder { DataSource = _databasePath };
        var connection = new SQLiteConnection(sqlConnectionStringBuilder.ToString());
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);

        using (await _asyncLock.LockAsync(cancellationToken))
        {
            using var connection = await this.GetConnectionAsync(cancellationToken);

            var query =
    @"
CREATE TABLE IF NOT EXISTS boxes (
    signature TEXT NOT NULL PRIMARY KEY,
    created_time INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS notes (
    self_hash TEXT NOT NULL PRIMARY KEY,
    signature TEXT NOT NULL,
    tag TEXT NOT NULL,
    created_time INTEGER NOT NULL,
    value BLOB NOT NULL
);
CREATE INDEX IF NOT EXISTS index_signature_and_created_time_for_boxes ON boxes (signature, created_time);
CREATE INDEX IF NOT EXISTS index_signature_for_notes ON notes (signature);
CREATE INDEX IF NOT EXISTS index_tag_for_notes ON notes (tag, created_time DESC);
CREATE INDEX IF NOT EXISTS index_created_time_for_notes ON notes (created_time);
";
            await connection.ExecuteNonQueryAsync(query, cancellationToken);
        }
    }

    public async ValueTask UpsertAsync(CachedNoteBox box, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);

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
DELETE FROM notes WHERE signature = @Signature;
";
                var parameters = new (string, object)[] {
                    ("@Signature", box.Signature.ToString()),
                    ("@CreatedTime", box.CreatedTime.Seconds)
                };

                await transaction.ExecuteNonQueryAsync(query, parameters, cancellationToken);
            }
        }

        lock (_lockObject)
        {
            using var connection = this.GetConnection();
            using var transaction = connection.BeginTransaction();

            {
                var signature = content.Signature.ToString();
                var shout_updated_time = content.ShoutUpdatedTime.Seconds;

                using var command = new SQLiteCommand(connection);
                command.CommandText =
$@"
DELETE FROM contents WHERE signature = '{signature}';
INSERT INTO contents (signature, shout_updated_time)
VALUES (
    '{signature}',
    '{shout_updated_time}'
);
DELETE FROM notes WHERE signature = '{signature}';
";
                command.ExecuteNonQuery();
            }

            using var bytesPipe = new BytesPipe(_bytesPool);

            foreach (var c in content.ToMemos())
            {
                var self_hash = c.SelfHash.ToString(ConvertStringType.Base16);
                var signature = c.Signature.ToString();
                var tag = c.Value.Tag.ToString();
                var created_time = c.Value.CreatedTime.Seconds;

                c.Export(bytesPipe.Writer, _bytesPool);
                var value = _base16.Value.BytesToString(bytesPipe.Reader.GetSequence());
                bytesPipe.Reset();

                using var command = new SQLiteCommand(connection);
                command.CommandText =
$@"
INSERT OR IGNORE INTO notes (self_hash, signature, tag, created_time, value)
VALUES (
    '{self_hash}',
    '{signature}',
    '{tag}',
    {created_time},
    x'{value}'
);
";
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public DateTime FetchShoutUpdatedTime(OmniSignature signature)
    {
        lock (_lockObject)
        {
            using var connection = this.GetConnection();
            var compiler = new SqliteCompiler();
            using var db = new QueryFactory(connection, compiler);

            var rows = db.Query("contents")
                .Select("shout_updated_time")
                .Where("signature", signature.ToString())
                .Get();

            if (rows.Count() == 0) return DateTime.MinValue;

            var shoutUpdatedTime = rows
                .Select(n => n.shout_updated_time)
                .OfType<long>()
                .Select(n => new Timestamp64(n).ToDateTime())
                .Single();
            return shoutUpdatedTime;
        }
    }

    public IEnumerable<CachedMemo> FetchMemoByTag(string tag)
    {
        lock (_lockObject)
        {
            using var connection = this.GetConnection();
            var compiler = new SqliteCompiler();
            using var db = new QueryFactory(connection, compiler);

            var rows = db.Query("messages")
                .Select("value")
                .Where("tag", tag)
                .OrderByDesc("created_time")
                .Get();

            var results = new List<CachedMemo>();

            foreach (var value in rows.Select(n => n.value).OfType<byte[]>())
            {
                var result = CachedMemo.Import(new ReadOnlySequence<byte>(value), _bytesPool);
                results.Add(result);
            }

            return results;
        }
    }

    public CachedMemo? FetchMemoBySelfHash(OmniHash selfHash)
    {
        lock (_lockObject)
        {
            using var connection = this.GetConnection();
            var compiler = new SqliteCompiler();
            using var db = new QueryFactory(connection, compiler);

            var rows = db.Query("messages")
                .Select("value")
                .Where("self_hash", selfHash.ToString())
                .Limit(1)
                .Get();

            var value = rows.Select(n => n.value).OfType<byte[]>().FirstOrDefault();
            if (value is null) return null;

            return CachedMemo.Import(new ReadOnlySequence<byte>(value), _bytesPool);
        }
    }

    public void Shrink(IEnumerable<OmniSignature> excludedSignatures)
    {
        lock (_lockObject)
        {
            using var connection = this.GetConnection();
            var compiler = new SqliteCompiler();
            using var db = new QueryFactory(connection, compiler);

            using var transaction = connection.BeginTransaction();

            var rows = db.Query("messages")
                .Select("signature")
                .Get();

            var allSignatureSet = rows.Select(n => n.signature).OfType<string>().Select(n => OmniSignature.Parse(n)).ToHashSet();
            allSignatureSet.ExceptWith(excludedSignatures);

            foreach (var signatures in allSignatureSet.Chunk(200))
            {
                db.Query("messages")
                    .WhereIn("signature", signatures.Select(n => n.ToString()))
                    .Delete();
            }

            transaction.Commit();
        }
    }
}
