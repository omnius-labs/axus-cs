using LiteDB;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Xeus.Intaractors.Internal.Entities;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Utils;

namespace Omnius.Xeus.Intaractors.Internal.Repositories;

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
        private const string CollectionName = "downloading_box_items";

        private readonly LiteDatabase _database;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        public DownloadingFileItemRepository(LiteDatabase database)
        {
            _database = database;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                if (_database.GetDocumentVersion(CollectionName) <= 0)
                {
                    var col = this.GetCollection();
                    col.EnsureIndex(x => x.Seed!.RootHash, true);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<DownloadingFileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<DownloadingFileItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(Seed seed)
        {
            using (_asyncLock.ReaderLock())
            {
                var seedEntity = SeedEntity.Import(seed);

                var col = this.GetCollection();
                return col.Exists(n => n.Seed == seedEntity);
            }
        }

        public bool Exists(OmniHash rootHash)
        {
            using (_asyncLock.ReaderLock())
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Exists(n => n.Seed != null && n.Seed.RootHash == rootHashEntity);
            }
        }

        public IEnumerable<DownloadingFileItem> FindAll()
        {
            using (_asyncLock.ReaderLock())
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public DownloadingFileItem? FindOne(Seed seed)
        {
            using (_asyncLock.ReaderLock())
            {
                var seedEntity = SeedEntity.Import(seed);

                var col = this.GetCollection();
                return col.FindOne(n => n.Seed == seedEntity).Export();
            }
        }

        public void Upsert(DownloadingFileItem item)
        {
            using (_asyncLock.WriterLock())
            {
                var itemEntity = DownloadingFileItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.Seed == itemEntity.Seed);
                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(Seed seed)
        {
            using (_asyncLock.WriterLock())
            {
                var seedEntity = SeedEntity.Import(seed);

                var col = this.GetCollection();
                col.DeleteMany(n => n.Seed == seedEntity);
            }
        }
    }
}
