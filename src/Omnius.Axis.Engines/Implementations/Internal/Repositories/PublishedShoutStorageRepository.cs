using LiteDB;
using Omnius.Axis.Engines.Internal.Entities;
using Omnius.Axis.Engines.Internal.Models;
using Omnius.Axis.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axis.Engines.Internal.Repositories;

internal sealed partial class PublishedShoutStorageRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public PublishedShoutStorageRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.Items = new PublishedShoutItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.Items.MigrateAsync(cancellationToken);
    }

    public PublishedShoutItemRepository Items { get; }

    public sealed class PublishedShoutItemRepository
    {
        private const string CollectionName = "published_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public PublishedShoutItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.Signature, false);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        public ILiteCollection<PublishedShoutItemEntity> GetCollection()
        {
            var col = _database.GetCollection<PublishedShoutItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                return col.Exists(n => n.Signature == signatureEntity);
            }
        }

        public IEnumerable<PublishedShoutItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export());
            }
        }

        public IEnumerable<PublishedShoutItem> Find(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                return col.Find(n => n.Signature == signatureEntity).Select(n => n.Export());
            }
        }

        public PublishedShoutItem? FindOne(OmniSignature signature, string registrant)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                return col.FindOne(n => n.Signature == signatureEntity && n.Registrant == registrant).Export();
            }
        }

        public void Insert(PublishedShoutItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = PublishedShoutItemEntity.Import(item);

                var col = this.GetCollection();

                if (col.Exists(n => n.Signature == itemEntity.Signature && n.Registrant == itemEntity.Registrant)) return;

                col.Insert(itemEntity);
            }
        }

        public void Delete(OmniSignature signature, string registrant)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                col.DeleteMany(n => n.Signature == signatureEntity && n.Registrant == registrant);
            }
        }
    }
}
