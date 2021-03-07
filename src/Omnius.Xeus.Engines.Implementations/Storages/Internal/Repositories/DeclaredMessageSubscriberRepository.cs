using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Xeus.Engines.Helpers;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories
{
    internal sealed partial class DeclaredMessageSubscriberRepository : DisposableBase
    {
        private readonly LiteDatabase _database;

        public DeclaredMessageSubscriberRepository(string filePath)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(filePath)!);

            _database = new LiteDatabase(filePath);
            this.Items = new SubscribedDeclaredMessageItemRepository(_database);
            this.WrittenItems = new WrittenDeclaredMessageItemRepository(_database);
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            await this.Items.MigrateAsync(cancellationToken);
            await this.WrittenItems.MigrateAsync(cancellationToken);
        }

        public SubscribedDeclaredMessageItemRepository Items { get; }

        public WrittenDeclaredMessageItemRepository WrittenItems { get; }

        public sealed class SubscribedDeclaredMessageItemRepository
        {
            private const string CollectionName = "items";

            private readonly LiteDatabase _database;

            private readonly AsyncReaderWriterLock _asyncLock = new();

            public SubscribedDeclaredMessageItemRepository(LiteDatabase database)
            {
                _database = database;
            }

            public ILiteCollection<SubscribedDeclaredMessageItemEntity> GetCollection()
            {
                var col = _database.GetCollection<SubscribedDeclaredMessageItemEntity>(CollectionName);
                return col;
            }

            internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    if (LiteDatabaseVersionHelper.GetVersion(_database, CollectionName) <= 0)
                    {
                        var col = this.GetCollection();
                        col.EnsureIndex(x => x.Signature, false);
                    }

                    LiteDatabaseVersionHelper.SetVersion(_database, CollectionName, 1);
                }
            }

            public bool Exists(OmniSignature signature)
            {
                using (_asyncLock.ReaderLock())
                {
                    var signatureEntity = OmniSignatureEntity.Import(signature);

                    var col = this.GetCollection();
                    return col.Exists(n => n.Signature == signatureEntity);
                }
            }

            public IEnumerable<SubscribedDeclaredMessageItem> FindAll()
            {
                using (_asyncLock.ReaderLock())
                {
                    var col = this.GetCollection();
                    return col.FindAll().Select(n => n.Export());
                }
            }

            public IEnumerable<SubscribedDeclaredMessageItem> Find(OmniSignature signature)
            {
                using (_asyncLock.ReaderLock())
                {
                    var signatureEntity = OmniSignatureEntity.Import(signature);

                    var col = this.GetCollection();
                    return col.Find(n => n.Signature == signatureEntity).Select(n => n.Export());
                }
            }

            public SubscribedDeclaredMessageItem? FindOne(OmniSignature signature, string registrant)
            {
                using (_asyncLock.ReaderLock())
                {
                    var signatureEntity = OmniSignatureEntity.Import(signature);

                    var col = this.GetCollection();
                    return col.FindOne(n => n.Signature == signatureEntity && n.Registrant == registrant).Export();
                }
            }

            public void Insert(SubscribedDeclaredMessageItem item)
            {
                using (_asyncLock.WriterLock())
                {
                    var itemEntity = SubscribedDeclaredMessageItemEntity.Import(item);

                    var col = this.GetCollection();

                    if (col.Exists(n => n.Signature == itemEntity.Signature && n.Registrant == itemEntity.Registrant)) return;

                    col.Insert(itemEntity);
                }
            }

            public void Delete(OmniSignature signature, string registrant)
            {
                using (_asyncLock.WriterLock())
                {
                    var signatureEntity = OmniSignatureEntity.Import(signature);

                    var col = this.GetCollection();
                    col.DeleteMany(n => n.Signature == signatureEntity && n.Registrant == registrant);
                }
            }
        }

        public sealed class WrittenDeclaredMessageItemRepository
        {
            private const string CollectionName = "written_items";

            private readonly LiteDatabase _database;

            private readonly AsyncReaderWriterLock _asyncLock = new();

            public WrittenDeclaredMessageItemRepository(LiteDatabase database)
            {
                _database = database;
            }

            public ILiteCollection<WrittenDeclaredMessageItemEntity> GetCollection()
            {
                var col = _database.GetCollection<WrittenDeclaredMessageItemEntity>(CollectionName);
                return col;
            }

            internal async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    if (LiteDatabaseVersionHelper.GetVersion(_database, CollectionName) <= 0)
                    {
                        var col = this.GetCollection();
                        col.EnsureIndex(x => x.Signature, true);
                    }

                    LiteDatabaseVersionHelper.SetVersion(_database, CollectionName, 1);
                }
            }

            public bool Exists(OmniSignature signature)
            {
                using (_asyncLock.ReaderLock())
                {
                    var signatureEntity = OmniSignatureEntity.Import(signature);

                    var col = this.GetCollection();
                    return col.Exists(n => n.Signature == signatureEntity);
                }
            }

            public IEnumerable<WrittenDeclaredMessageItem> FindAll()
            {
                using (_asyncLock.ReaderLock())
                {
                    var col = this.GetCollection();
                    return col.FindAll().Select(n => n.Export());
                }
            }

            public WrittenDeclaredMessageItem? FindOne(OmniSignature signature)
            {
                using (_asyncLock.ReaderLock())
                {
                    var signatureEntity = OmniSignatureEntity.Import(signature);

                    var col = this.GetCollection();
                    return col.FindOne(n => n.Signature == signatureEntity).Export();
                }
            }

            public void Insert(WrittenDeclaredMessageItem item)
            {
                using (_asyncLock.WriterLock())
                {
                    var itemEntity = WrittenDeclaredMessageItemEntity.Import(item);

                    var col = this.GetCollection();

                    if (col.Exists(n => n.Signature == itemEntity.Signature)) return;

                    col.Insert(itemEntity);
                }
            }

            public void Delete(OmniSignature signature)
            {
                using (_asyncLock.WriterLock())
                {
                    var signatureEntity = OmniSignatureEntity.Import(signature);

                    var col = this.GetCollection();
                    col.DeleteMany(n => n.Signature == signatureEntity);
                }
            }
        }
    }
}
