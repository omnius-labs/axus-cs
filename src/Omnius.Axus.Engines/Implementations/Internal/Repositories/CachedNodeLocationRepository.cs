using LiteDB;
using Omnius.Axus.Engines.Internal.Entities;
using Omnius.Axus.Messages;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Engines.Internal.Repositories;

internal sealed class CachedNodeLocationRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    private readonly object _lockObject = new();

    private const string CollectionName = "cached_node_locations";

    public CachedNodeLocationRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (_database.GetDocumentVersion(CollectionName) <= 0)
            {
                var col = this.GetCollection();
                col.EnsureIndex(x => x.Value, true);
            }

            _database.SetDocumentVersion(CollectionName, 1);
        }
    }

    private ILiteCollection<CachedNodeLocationEntity> GetCollection()
    {
        var col = _database.GetCollection<CachedNodeLocationEntity>(CollectionName);
        return col;
    }

    public IEnumerable<NodeLocation> FindAll()
    {
        lock (_lockObject)
        {
            var col = this.GetCollection();
            return col.FindAll().Select(n => n.Value!.Export()).ToArray();
        }
    }

    public void Insert(NodeLocation nodeLocation, DateTime updatedTime)
    {
        this.InsertBulk(new[] { nodeLocation }, updatedTime);
    }

    public void InsertBulk(IEnumerable<NodeLocation> nodeLocations, DateTime updatedTime)
    {
        lock (_lockObject)
        {
            var col = this.GetCollection();

            _database.BeginTrans();

            foreach (var value in nodeLocations)
            {
                var itemEntity = new CachedNodeLocationEntity()
                {
                    Value = NodeLocationEntity.Import(value),
                    LastConnectedTime = DateTime.MinValue,
                    UpdatedTime = updatedTime,
                };

                if (col.Exists(n => n.Value == itemEntity.Value)) continue;

                col.Insert(itemEntity);
            }

            _database.Commit();
        }
    }

    public void Upsert(NodeLocation nodeLocation, DateTime updatedTime, DateTime lastConnectedTime)
    {
        this.UpsertBulk(new[] { nodeLocation }, updatedTime, lastConnectedTime);
    }

    public void UpsertBulk(IEnumerable<NodeLocation> nodeLocations, DateTime updatedTime, DateTime lastConnectedTime)
    {
        lock (_lockObject)
        {
            var col = this.GetCollection();

            _database.BeginTrans();

            foreach (var value in nodeLocations)
            {
                var itemEntity = new CachedNodeLocationEntity()
                {
                    Value = NodeLocationEntity.Import(value),
                    LastConnectedTime = lastConnectedTime,
                    UpdatedTime = updatedTime,
                };

                col.DeleteMany(n => n.Value == itemEntity.Value);
                col.Insert(itemEntity);
            }

            _database.Commit();
        }
    }

    public void Shrink(int capacity)
    {
        lock (_lockObject)
        {
            var col = this.GetCollection();

            _database.BeginTrans();

            foreach (var extra in col.FindAll().OrderBy(n => n.UpdatedTime).OrderByDescending(n => n.LastConnectedTime).Skip(capacity).ToArray())
            {
                col.DeleteMany(n => n.Value == extra.Value);
            }

            _database.Commit();
        }
    }

    public int Count()
    {
        lock (_lockObject)
        {
            var col = this.GetCollection();
            return col.Count();
        }
    }
}
