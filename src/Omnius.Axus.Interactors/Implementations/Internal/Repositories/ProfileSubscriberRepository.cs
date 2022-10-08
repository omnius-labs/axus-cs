using LiteDB;
using Omnius.Axus.Interactors.Internal.Entities;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Interactors.Internal.Repositories;

internal sealed class ProfileSubscriberRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public ProfileSubscriberRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.ProfileItems = new SubscribedProfileItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.ProfileItems.MigrateAsync(cancellationToken);
    }

    public SubscribedProfileItemRepository ProfileItems { get; }

    public sealed class SubscribedProfileItemRepository
    {
        private const string CollectionName = "subscribed_profile_items";

        private readonly LiteDatabase _database;

        private readonly object _lockObject = new();

        public SubscribedProfileItemRepository(LiteDatabase database)
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
                    col.EnsureIndex(x => x.RootHash, false);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<SubscribedProfileItemEntity> GetCollection()
        {
            var col = _database.GetCollection<SubscribedProfileItemEntity>(CollectionName);
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

        public bool Exists(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                return col.Exists(n => n.RootHash == rootHashEntity);
            }
        }

        public IEnumerable<SubscribedProfileItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public SubscribedProfileItem? FindOne(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                return col.FindOne(n => n.Signature == signatureEntity)?.Export();
            }
        }

        public void Upsert(SubscribedProfileItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = SubscribedProfileItemEntity.Import(item);

                var col = this.GetCollection();

                col.DeleteMany(n => n.Signature == itemEntity.Signature && n.RootHash == itemEntity.RootHash);
                col.Insert(itemEntity);
            }
        }

        public void Delete(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                col.DeleteMany(n => n.Signature == signatureEntity);
            }
        }

        public void Delete(OmniHash rootHash)
        {
            lock (_lockObject)
            {
                var rootHashEntity = OmniHashEntity.Import(rootHash);

                var col = this.GetCollection();
                col.DeleteMany(n => n.RootHash == rootHashEntity);
            }
        }
    }
}
