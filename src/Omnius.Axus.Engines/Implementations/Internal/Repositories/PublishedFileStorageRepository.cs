using LiteDB;
using Omnius.Axus.Engines.Internal.Entities;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Engines.Internal.Repositories;

internal sealed class PublishedFileStorageRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public PublishedFileStorageRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.Items = new PublishedFileItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.Items.MigrateAsync(cancellationToken);
    }

    public PublishedFileItemRepository Items { get; }

    public sealed class PublishedFileItemRepository
    {
        private const string CollectionName = "published_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public PublishedFileItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.RootHash, false);
                    col.EnsureIndex(x => x.FilePath, false);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<PublishedFileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<PublishedFileItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Exists(n => n.RootHash == rootHashEntity);
            }
        }

        public bool Exists(string filePath)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.Exists(n => n.FilePath == filePath);
            }
        }

        public IEnumerable<PublishedFileItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<PublishedFileItem> Find(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export()).ToArray();
            }
        }

        public PublishedFileItem? FindOne(string filePath, string registrant)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindOne(n => n.FilePath == filePath && n.Registrant == registrant)?.Export();
            }
        }

        public PublishedFileItem? FindOne(OmniHash rootHash, string registrant)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.RootHash == rootHashEntity && n.FilePath == null && n.Registrant == registrant)?.Export();
            }
        }

        public void Upsert(PublishedFileItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = PublishedFileItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                if (item.FilePath is not null)
                {
                    col.DeleteMany(n => n.FilePath == item.FilePath && n.Registrant == item.Registrant);
                }
                else
                {
                    col.DeleteMany(n => n.RootHash == itemEntity.RootHash && n.FilePath == null && n.Registrant == item.Registrant);
                }

                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(OmniHash rootHash, string registrant)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                col.DeleteMany(n => n.RootHash == rootHashEntity && n.FilePath == null && n.Registrant == registrant);
            }
        }

        public void Delete(string filePath, string registrant)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                col.DeleteMany(n => n.FilePath == filePath && n.Registrant == registrant);
            }
        }
    }
}
