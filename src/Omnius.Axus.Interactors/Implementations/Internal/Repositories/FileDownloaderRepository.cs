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

        this.Items = new DownloadingFileItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.Items.MigrateAsync(cancellationToken);
    }

    public DownloadingFileItemRepository Items { get; }

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
                    col.EnsureIndex(x => x.FileSeed!.RootHash, true);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<DownloadingFileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<DownloadingFileItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(FileSeed fileSeed)
        {
            lock (_lockObject)
            {
                var seedEntity = FileSeedEntity.Import(fileSeed);

                var col = this.GetCollection();
                return col.Exists(n => n.FileSeed == seedEntity);
            }
        }

        public bool Exists(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Exists(n => n.FileSeed!.RootHash == rootHashEntity);
            }
        }

        public IEnumerable<DownloadingFileItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public DownloadingFileItem? FindOne(FileSeed fileSeed)
        {
            lock (_lockObject)
            {
                var seedEntity = FileSeedEntity.Import(fileSeed);

                var col = this.GetCollection();
                return col.FindOne(n => n.FileSeed == seedEntity)?.Export();
            }
        }

        public void Upsert(DownloadingFileItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = DownloadingFileItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.FileSeed!.RootHash == itemEntity.FileSeed!.RootHash);
                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(FileSeed fileSeed)
        {
            lock (_lockObject)
            {
                var seedEntity = FileSeedEntity.Import(fileSeed);

                var col = this.GetCollection();
                col.DeleteMany(n => n.FileSeed!.RootHash == seedEntity.RootHash);
            }
        }
    }
}
