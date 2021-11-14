using LiteDB;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Xeus.Service.Engines.Internal.Entities;
using Omnius.Xeus.Service.Engines.Internal.Models;
using Omnius.Xeus.Utils;

namespace Omnius.Xeus.Service.Engines.Internal.Repositories;

internal sealed partial class SubscribedFileStorageRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public SubscribedFileStorageRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.Items = new SubscribedFileItemRepository(_database);
        this.DecodedItems = new DecodedFileItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.Items.MigrateAsync(cancellationToken);
        await this.DecodedItems.MigrateAsync(cancellationToken);
    }

    public SubscribedFileItemRepository Items { get; }

    public DecodedFileItemRepository DecodedItems { get; }

    public sealed class SubscribedFileItemRepository
    {
        private const string CollectionName = "subscribed_items";

        private readonly LiteDatabase _database;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        public SubscribedFileItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.RootHash, false);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<SubscribedFileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<SubscribedFileItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(OmniHash rootHash)
        {
            using (_asyncLock.ReaderLock())
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Exists(n => n.RootHash == rootHashEntity);
            }
        }

        public IEnumerable<SubscribedFileItem> FindAll()
        {
            using (_asyncLock.ReaderLock())
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public IEnumerable<SubscribedFileItem> Find(OmniHash rootHash)
        {
            using (_asyncLock.ReaderLock())
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Find(n => n.RootHash == rootHashEntity).Select(n => n.Export());
            }
        }

        public SubscribedFileItem? FindOne(OmniHash rootHash, string registrant)
        {
            using (_asyncLock.ReaderLock())
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.RootHash == rootHashEntity && n.Registrant == registrant)?.Export();
            }
        }

        public void Upsert(SubscribedFileItem item)
        {
            using (_asyncLock.WriterLock())
            {
                var itemEntity = SubscribedFileItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.RootHash == itemEntity.RootHash && n.Registrant == itemEntity.Registrant);
                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(OmniHash rootHash, string registrant)
        {
            using (_asyncLock.WriterLock())
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                col.DeleteMany(n => n.RootHash == rootHashEntity && n.Registrant == registrant);
            }
        }
    }

    public sealed class DecodedFileItemRepository
    {
        private const string CollectionName = "decoded_items";

        private readonly LiteDatabase _database;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        public DecodedFileItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.RootHash, true);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<DecodedFileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<DecodedFileItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(OmniHash rootHash)
        {
            using (_asyncLock.ReaderLock())
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Exists(n => n.RootHash == rootHashEntity);
            }
        }

        public IEnumerable<DecodedFileItem> FindAll()
        {
            using (_asyncLock.ReaderLock())
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public DecodedFileItem? FindOne(OmniHash rootHash)
        {
            using (_asyncLock.ReaderLock())
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.RootHash == rootHashEntity)?.Export();
            }
        }

        public void Upsert(DecodedFileItem item)
        {
            using (_asyncLock.WriterLock())
            {
                var itemEntity = DecodedFileItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.RootHash == itemEntity.RootHash);
                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(OmniHash rootHash)
        {
            using (_asyncLock.WriterLock())
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                col.DeleteMany(n => n.RootHash == rootHashEntity);
            }
        }
    }
}
