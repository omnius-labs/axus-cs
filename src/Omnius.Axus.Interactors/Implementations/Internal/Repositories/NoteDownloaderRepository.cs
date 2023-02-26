using LiteDB;
using Omnius.Axus.Interactors.Internal.Entities;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Utils;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Axus.Interactors.Internal.Repositories;

internal sealed class NoteDownloaderRepository : DisposableBase
{
    private readonly LiteDatabase _database;

    public NoteDownloaderRepository(string dirPath)
    {
        DirectoryHelper.CreateDirectory(dirPath);

        _database = new LiteDatabase(Path.Combine(dirPath, "lite.db"));
        _database.UtcDate = true;

        this.BarkItems = new SubscribedBarkItemRepository(_database);
    }

    protected override void OnDispose(bool disposing)
    {
        _database.Dispose();
    }

    public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
    {
        await this.BarkItems.MigrateAsync(cancellationToken);
    }

    public SubscribedBarkItemRepository BarkItems { get; }

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
                    col.EnsureIndex(x => x.Signature, false);
                    col.EnsureIndex(x => x.RootHash, false);
                }

                _database.SetDocumentVersion(CollectionName, 1);
            }
        }

        private ILiteCollection<NoteDownloadingItemEntity> GetCollection()
        {
            var col = _database.GetCollection<NoteDownloadingItemEntity>(CollectionName);
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

        public IEnumerable<NoteDownloadingItem> FindAll()
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Export()).ToArray();
            }
        }

        public NoteDownloadingItem? FindOne(OmniSignature signature)
        {
            lock (_lockObject)
            {
                var signatureEntity = OmniSignatureEntity.Import(signature);

                var col = this.GetCollection();
                return col.FindOne(n => n.Signature == signatureEntity)?.Export();
            }
        }

        public void Upsert(NoteDownloadingItem item)
        {
            lock (_lockObject)
            {
                var itemEntity = NoteDownloadingItemEntity.Import(item);

                var col = this.GetCollection();

                col.DeleteMany(n => n.Signature == itemEntity.Signature);
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

        public void Shrink(IEnumerable<OmniSignature> excludedSignatures)
        {
            lock (_lockObject)
            {
                var col = this.GetCollection();

                var allSignatureSet = col.FindAll().Select(n => n.Signature!.Export()).ToHashSet();
                allSignatureSet.ExceptWith(excludedSignatures);

                foreach (var signature in allSignatureSet)
                {
                    var signatureEntity = OmniSignatureEntity.Import(signature);
                    col.DeleteMany(n => n.Signature == signatureEntity);
                }
            }
        }
    }
}
