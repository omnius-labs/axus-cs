using LiteDB;
using Omnius.Axis.Intaractors.Internal.Entities;
using Omnius.Axis.Intaractors.Internal.Models;
using Omnius.Axis.Utils;
using Omnius.Core;
using Omnius.Core.Helpers;

namespace Omnius.Axis.Intaractors.Internal.Repositories;

internal sealed class FileUploaderRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public FileUploaderRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.Items = new UploadingFileItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.Items.MigrateAsync(cancellationToken);
    }

    public UploadingFileItemRepository Items { get; }

    public sealed class UploadingFileItemRepository
    {
        private const string CollectionName = "uploading_file_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public UploadingFileItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.FilePath, true);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<UploadingFileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<UploadingFileItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(string filePath)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.Exists(n => n.FilePath == filePath);
            }
        }

        public IEnumerable<UploadingFileItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public UploadingFileItem? FindOne(string filePath)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindById(filePath).Export();
            }
        }

        public void Upsert(UploadingFileItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = UploadingFileItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.FilePath == item.FilePath);
                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(string filePath)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                col.DeleteMany(n => n.FilePath == filePath);
            }
        }
    }
}
