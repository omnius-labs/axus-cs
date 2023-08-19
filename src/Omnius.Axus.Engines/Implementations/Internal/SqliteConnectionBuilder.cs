using System.Data.SQLite;

namespace Omnius.Axus.Engines.Internal;

internal class SQLiteConnectionBuilder
{
    private readonly string _path;

    public SQLiteConnectionBuilder(string path)
    {
        _path = path;
    }

    public async ValueTask<SQLiteConnection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var connectionStringBuilder = new SQLiteConnectionStringBuilder
        {
            DataSource = _path,
            ForeignKeys = true,
        };
        var connection = new SQLiteConnection(connectionStringBuilder.ToString());
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
