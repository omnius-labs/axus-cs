using LiteDB;
using Omnius.Axis.Engines.Internal.Entities;
using Omnius.Axis.Models;
using Omnius.Axis.Utils;
using Omnius.Core;
using Omnius.Core.Helpers;

namespace Omnius.Axis.Engines.Internal.Repositories;

internal sealed class NodeFinderRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public NodeFinderRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.NodeLocations = new CachedNodeLocationRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.NodeLocations.MigrateAsync(cancellationToken);
    }

    public CachedNodeLocationRepository NodeLocations { get; }

    public sealed class CachedNodeLocationRepository
    {
        private const string CollectionName = "cached_node_locations";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public CachedNodeLocationRepository(LiteDatabase database)
        {
            _database = database;
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
                return col.FindAll().Where(n => n.Value != null).Select(n => n.Value!.Export()).ToArray();
            }
        }

        public bool TryInsert(NodeLocation value, DateTime createdTime)
        {
            lock (_lockObject)
            {
                var itemEntity = new CachedNodeLocationEntity()
                {
                    Value = NodeLocationEntity.Import(value),
                    CreatedTime = createdTime,
                    LastConnectionTime = DateTime.MinValue
                };

                var col = this.GetCollection();

                if (col.Exists(n => n.Value == itemEntity.Value)) return false;

                col.Insert(itemEntity);
                return true;
            }
        }

        public void Upsert(NodeLocation value, DateTime createdTime, DateTime lastConnectionTime)
        {
            lock (_lockObject)
            {
                var itemEntity = new CachedNodeLocationEntity()
                {
                    Value = NodeLocationEntity.Import(value),
                    CreatedTime = createdTime,
                    LastConnectionTime = lastConnectionTime
                };

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.Value == itemEntity.Value);
                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void TrimExcess(int capacity)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();

                _database.BeginTrans();

                foreach (var extra in col.FindAll().OrderBy(n => n.CreatedTime).OrderByDescending(n => n.LastConnectionTime).Skip(capacity).ToArray())
                {
                    col.DeleteMany(n => n.Value == extra.Value);
                }

                _database.Commit();
            }
        }
    }
}
