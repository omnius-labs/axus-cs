using System.Buffers;
using System.Data.SQLite;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Pipelines;
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
CREATE TABLE IF NOT EXISTS cached_bark_messages (
    self_hash TEXT NOT NULL PRIMARY KEY,
    signature TEXT NOT NULL,
    tag TEXT NOT NULL,
    created_time INTEGER NOT NULL,
    value BLOB NOT NULL
);
CREATE INDEX IF NOT EXISTS index_signature_for_cached_bark_messages ON cached_bark_messages (signature);
CREATE INDEX IF NOT EXISTS index_tag_for_cached_bark_messages ON cached_bark_messages (tag, created_time DESC);
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

    public void InsertBulk(IEnumerable<CachedBarkMessage> messages)
    {
        // SqlKataが"INSERT OR IGNORE"に非対応のため、自前でSQL文を生成する

        using var connection = this.GetConnection();
        using var transaction = connection.BeginTransaction();

        using var bytesPipe = new BytesPipe(_bytesPool);

        foreach (var m in messages)
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
INSERT OR IGNORE INTO cached_bark_messages(self_hash, signature, tag, created_time, value)
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

    public IEnumerable<CachedBarkMessage> FetchByTag(string tag)
    {
        using var connection = this.GetConnection();
        var compiler = new SqliteCompiler();
        using var db = new QueryFactory(connection, compiler);

        var rows = db.Query("cached_bark_messages")
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

    public CachedBarkMessage? FetchBySelfHash(OmniHash selfHash)
    {
        using var connection = this.GetConnection();
        var compiler = new SqliteCompiler();
        using var db = new QueryFactory(connection, compiler);

        var rows = db.Query("cached_bark_messages")
            .Select("value")
            .Where("self_hash", selfHash.ToString())
            .Limit(1)
            .Get();

        var value = rows.Select(n => n.value).OfType<byte[]>().FirstOrDefault();
        if (value is null) return null;

        return CachedBarkMessage.Import(new ReadOnlySequence<byte>(value), _bytesPool);
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
