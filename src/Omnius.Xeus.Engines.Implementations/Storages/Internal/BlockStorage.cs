using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Helpers;

namespace Omnius.Xeus.Engines.Storages.Internal
{
    internal sealed class BlockStogage : DisposableBase
    {
        private readonly IBytesPool _bytesPool;
        private readonly LiteDatabase _database;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        public BlockStogage(string filePath, IBytesPool bytesPool)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(filePath)!);

            _database = new LiteDatabase(filePath);
            _bytesPool = bytesPool;
        }

        protected override void OnDispose(bool disposing)
        {
            _database.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                if (_database.UserVersion <= 0)
                {
                    var col = this.GetCollection();
                    col.EnsureIndex(n => n.Name, true);
                }
            }
        }

        public async ValueTask RebuildAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _database.Rebuild();
            }
        }

        private ILiteStorage<long> GetStorage()
        {
            var storage = _database.GetStorage<long>();
            return storage;
        }

        private ILiteCollection<BlockMeta> GetCollection()
        {
            var col = _database.GetCollection<BlockMeta>();
            return col;
        }

        public async ValueTask RenameAsync(string oldName, string newName, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var col = this.GetCollection();
                var meta = col.FindOne(n => n.Name == oldName);
                if (meta is null)
                {
                    throw new KeyNotFoundException();
                }

                meta.Name = newName;

                col.Update(meta);
            }
        }

        public async ValueTask<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var col = this.GetCollection();
                return col.Exists(name);
            }
        }

        public async ValueTask<IEnumerable<string>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var col = this.GetCollection();
                return col.FindAll().Select(n => n.Name!).ToArray();
            }
        }

        public async ValueTask WriteAsync(string name, ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                if (name is null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var col = this.GetCollection();
                var id = col.Insert(new BlockMeta() { Name = name }).AsInt64;

                var storage = this.GetStorage();
                await using var liteFileStream = storage.OpenWrite(id, "-");

                foreach (var memory in sequence)
                {
                    await liteFileStream.WriteAsync(memory, cancellationToken);
                }
            }
        }

        public async ValueTask WriteAsync(string name, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            await this.WriteAsync(name, new ReadOnlySequence<byte>(memory), cancellationToken);
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadAsync(string name, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var col = this.GetCollection();
                var meta = col.FindOne(n => n.Name == name);
                if (meta is null)
                {
                    return null;
                }

                var storage = this.GetStorage();
                await using var liteFileStream = storage.OpenRead(meta.Id);

                var memoryOwner = _bytesPool.Memory.Rent((int)liteFileStream.Length);

                while (liteFileStream.Position < liteFileStream.Length)
                {
                    var memory = memoryOwner.Memory;
                    await liteFileStream.ReadAsync(memory[(int)liteFileStream.Position..], cancellationToken);
                }

                return memoryOwner;
            }
        }

        public async ValueTask DeleteAsync(string name, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var col = this.GetCollection();
                var meta = col.FindOne(n => n.Name == name);
                if (meta is null)
                {
                    return;
                }

                var storage = this.GetStorage();
                storage.Delete(meta.Id);

                col.Delete(meta.Id);
            }
        }

        private sealed class BlockMeta
        {
            public long Id { get; set; }

            public string? Name { get; set; }
        }
    }
}
