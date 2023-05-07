using LiteDB;
using Omnius.Axus.Engines.Internal.Entities;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Engines.Internal.Repositories;

internal sealed partial class ShoutPublisherStorageRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public ShoutPublisherStorageRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.ShoutItems = new ShoutPublishedItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.ShoutItems.MigrateAsync(cancellationToken);
    }

    public ShoutPublishedItemRepository ShoutItems { get; }

    public sealed class ShoutPublishedItemRepository
    {
        private const string CollectionName = "published_shout_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public ShoutPublishedItemRepository(LiteDatabase database)
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

        public ILiteCollection<ShoutPublishedItemEntity> GetCollection()
        {
            var col = _database.GetCollection<ShoutPublishedItemEntity>(CollectionName);
            return col;
        }

        public bool Exists(OmniSignature signature, string channel)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                return col.Exists(n => n.Signature == signatureEntity && n.Channel == channel);
            }
        }

        public IEnumerable<ShoutPublishedItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export());
            }
        }

        public IEnumerable<ShoutPublishedItem> Find(string zone)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.Find(n => n.Zones!.Contains(zone)).Select(n => n.Export());
            }
        }

        public ShoutPublishedItem? FindOne(OmniSignature signature, string channel)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                return col.FindOne(n => n.Signature == signatureEntity && n.Channel == channel)?.Export();
            }
        }

        public void Upsert(ShoutPublishedItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = ShoutPublishedItemEntity.Import(item);

                var col = this.GetCollection();

                _database.BeginTrans();

                col.DeleteMany(n => n.Signature == itemEntity.Signature && n.Channel == item.Channel);
                col.Insert(itemEntity);

                _database.Commit();
            }
        }

        public void Delete(OmniSignature signature, string channel)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                col.DeleteMany(n => n.Signature == signatureEntity && n.Channel == channel);
            }
        }
    }
}
