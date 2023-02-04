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

internal sealed class CachedBarkMessageRepository
{
    private readonly string _databasePath;
    private readonly IBytesPool _bytesPool;

    private static readonly Lazy<Base16> _base16 = new Lazy<Base16>(() => new Base16(ConvertStringCase.Lower));

    private readonly object _lockObject = new();

    public CachedBarkMessageRepository(string dirPath, IBytesPool bytesPool)
    {
        DirectoryHelper.CreateDirectory(dirPath);
        _databasePath = Path.Combine(dirPath, "sqlite.db");
        _bytesPool = bytesPool;
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        using var connection = this.GetConnection();
        using var command = new SQLiteCommand(connection);
        command.CommandText =
@"
CREATE TABLE IF NOT EXISTS contents (
    signature TEXT NOT NULL PRIMARY KEY,
    shout_updated_time INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS messages (
    self_hash TEXT NOT NULL PRIMARY KEY,
    signature TEXT NOT NULL,
    tag TEXT NOT NULL,
    created_time INTEGER NOT NULL,
    value BLOB NOT NULL
);
CREATE INDEX IF NOT EXISTS index_signature_for_messages ON messages (signature);
CREATE INDEX IF NOT EXISTS index_tag_for_messages ON messages (tag, created_time DESC);
";
        command.ExecuteNonQuery();
    }

    private SQLiteConnection GetConnection()
    {
        var sqlConnectionStringBuilder = new SQLiteConnectionStringBuilder { DataSource = _databasePath };
        var connection = new SQLiteConnection(sqlConnectionStringBuilder.ToString());
        connection.Open();
        return connection;
    }

    public void UpsertBulk(CachedBarkContent content)
    {
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
DELETE FROM messages WHERE signature = '{signature}';
";
                command.ExecuteNonQuery();
            }

            using var bytesPipe = new BytesPipe(_bytesPool);

            foreach (var m in content.ToMessages())
            {
                var self_hash = m.SelfHash.ToString(ConvertStringType.Base16);
                var signature = m.Signature.ToString();
                var tag = m.Value.Tag.ToString();
                var created_time = m.Value.CreatedTime.Seconds;

                m.Export(bytesPipe.Writer, _bytesPool);
                var value = _base16.Value.BytesToString(bytesPipe.Reader.GetSequence());
                bytesPipe.Reset();

                using var command = new SQLiteCommand(connection);
                command.CommandText =
$@"
INSERT OR IGNORE INTO messages (self_hash, signature, tag, created_time, value)
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

    public IEnumerable<CachedBarkMessage> FetchMessageByTag(string tag)
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

            var results = new List<CachedBarkMessage>();

            foreach (var value in rows.Select(n => n.value).OfType<byte[]>())
            {
                var result = CachedBarkMessage.Import(new ReadOnlySequence<byte>(value), _bytesPool);
                results.Add(result);
            }

            return results;
        }
    }

    public CachedBarkMessage? FetchMessageBySelfHash(OmniHash selfHash)
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

            return CachedBarkMessage.Import(new ReadOnlySequence<byte>(value), _bytesPool);
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

            var rows = db.Query("cached_bark_messages")
                .Select("signature")
                .Get();

            var allSignatureSet = rows.Select(n => n.signature).OfType<string>().Select(n => OmniSignature.Parse(n)).ToHashSet();
            allSignatureSet.ExceptWith(excludedSignatures);

            foreach (var signatures in allSignatureSet.Chunk(200))
            {
                db.Query("cached_bark_messages")
                    .WhereIn("signature", signatures.Select(n => n.ToString()))
                    .Delete();
            }

            transaction.Commit();
        }
    }
}
