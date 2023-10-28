using LiteDB;
using Omnius.Axus.Interactors.Internal.Entities;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Interactors.Internal.Repositories;

internal sealed class FileDownloaderRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public FileDownloaderRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.FileItems = new DownloadingFileItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.FileItems.MigrateAsync(cancellationToken);
    }

    public DownloadingFileItemRepository FileItems { get; }

    public sealed class DownloadingFileItemRepository
    {
        private const string CollectionName = "downloading_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public DownloadingFileItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.Seed!.RootHash, true);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<FileDownloadingItemEntity> GetCollection()
        {
            var col = _database.GetCollection<FileDownloadingItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(Seed seed)
        {
            lock (_lockObject)
            {
                var seedEntity = SeedEntity.Import(seed);

                var col = this.GetCollection();
                return col.Exists(n => n.Seed == seedEntity);
            }
        }

        public bool Exists(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Exists(n => n.Seed!.RootHash == rootHashEntity);
            }
        }

        public IEnumerable<FileDownloadingItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public FileDownloadingItem? FindOne(Seed seed)
        {
            lock (_lockObject)
            {
                var seedEntity = SeedEntity.Import(seed);

                var col = this.GetCollection();
                return col.FindOne(n => n.Seed == seedEntity)?.Export();
            }
        }

        public void Upsert(FileDownloadingItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = FileDownloadingItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.Seed!.RootHash == itemEntity.Seed!.RootHash);
                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(Seed seed)
        {
            lock (_lockObject)
            {
                var seedEntity = SeedEntity.Import(seed);

                var col = this.GetCollection();
                col.DeleteMany(n => n.Seed!.RootHash == seedEntity.RootHash);
            }
        }
    }
}
