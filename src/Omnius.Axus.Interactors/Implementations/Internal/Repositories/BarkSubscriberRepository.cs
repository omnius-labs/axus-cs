using LiteDB;
using Omnius.Axus.Interactors.Internal.Entities;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Interactors.Internal.Repositories;

internal sealed class BarkSubscriberRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public BarkSubscriberRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.Metadatas = new SubscribedBarkPackageMetadataRepository(_database);
        this.Items = new SubscribedBarkItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.Items.MigrateAsync(cancellationToken);
    }

    public SubscribedBarkPackageMetadataRepository Metadatas { get; }

    public SubscribedBarkItemRepository Items { get; }

    public sealed class SubscribedBarkPackageMetadataRepository
    {
        private const string CollectionName = "metadatas";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public SubscribedBarkPackageMetadataRepository(LiteDatabase database)
        {
            _database = database;
        }

        internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
        }

        private ILiteCollection<SubscribedBarkPackageMetadataEntity> GetCollection()
        {
            var col = _database.GetCollection<SubscribedBarkPackageMetadataEntity>(CollectionName);
            return col;
        }
    }

    public sealed class SubscribedBarkItemRepository
    {
        private const string CollectionName = "subscribed_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public SubscribedBarkItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.Tag, false);
                    col.EnsureIndex(x => x.Signature, false);
                    col.EnsureIndex(x => x.SelfHash, true);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<SubscribedBarkItemEntity> GetCollection()
        {
            var col = _database.GetCollection<SubscribedBarkItemEntity>(CollectionName);
            return col;
        }

        public IEnumerable<SubscribedBarkItem> FindByTag(string tag)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public SubscribedBarkItem? FindBySelfHash(OmniHash selfHash)
        {
            lock (_lockObject)
            {
                var selfHashEntity = OmniHashEntity.Import(selfHash);

                var col = this.GetCollection();
                return col.FindOne(n => n.SelfHash == selfHashEntity)?.Export();
            }
        }

        public void InsertBulk(IEnumerable<SubscribedBarkItem> items)
        {
            lock (_lockObject)
            {
                var itemEntities = items.Select(n => SubscribedBarkItemEntity.Import(n)).ToArray();

                var col = this.GetCollection();
                col.InsertBulk(itemEntities);
            }
        }

        public void DeleteBySignature(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                col.DeleteMany(n => n.Signature == signatureEntity);
            }
        }
    }
}
