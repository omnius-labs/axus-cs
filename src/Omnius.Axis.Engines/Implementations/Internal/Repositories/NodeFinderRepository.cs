using LiteDB;
using Omnius.Axis.Engines.Internal.Entities;
using Omnius.Axis.Models;
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

        this.NodeLocations = new NodeLocationRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.NodeLocations.MigrateAsync(cancellationToken);
    }

    public NodeLocationRepository NodeLocations { get; }

    public sealed class NodeLocationRepository
    {
        private const string CollectionName = "node_locations";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public NodeLocationRepository(LiteDatabase database)
        {
            _database = database;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
            }
        }

        private ILiteCollection<NodeLocationEntity> GetCollection()
        {
            var col = _database.GetCollection<NodeLocationEntity>(CollectionName);
            return col;
        }

        public IEnumerable<NodeLocation> Load()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public void Save(IEnumerable<NodeLocation> nodeLocations)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                col.DeleteAll();
                col.InsertBulk(nodeLocations.Select(n => NodeLocationEntity.Import(n)));
            }
        }
    }
}
