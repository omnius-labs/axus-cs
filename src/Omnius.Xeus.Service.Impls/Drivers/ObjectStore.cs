using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Extensions;
using Omnius.Core.Io;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Connections.Secure;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Core.Serialization.RocketPack;
using Omnius.Xeus.Service.Drivers.Internal;

namespace Omnius.Xeus.Service.Drivers
{
    public sealed class ObjectStore : DisposableBase, IObjectStore
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly FileStream _lockFileStream;

        private readonly ObjectStoreOptions _options;
        private readonly IBytesPool _bytesPool;

        internal sealed class ObjectStoreFactory : IObjectStoreFactory
        {
            public IObjectStore Create(ObjectStoreOptions options, IBytesPool bytesPool)
            {
                return new ObjectStore(options, bytesPool);
            }
        }

        public static IObjectStoreFactory Factory { get; } = new ObjectStoreFactory();

        internal ObjectStore(ObjectStoreOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            // 排他ロック
            _lockFileStream = new FileStream(Path.Combine(_options.DirectoryPath, "lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            // フォルダを作成する
            var directoryPathList = new List<string>();
            directoryPathList.Add(_options.DirectoryPath);
            directoryPathList.Add(this.GeneratePath(EntityStatus.Temp));
            directoryPathList.Add(this.GeneratePath(EntityStatus.Committed));
            directoryPathList.Add(this.GeneratePath(EntityStatus.Backup));

            foreach (var path in directoryPathList)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _lockFileStream.Dispose();
            }
        }

        public void Scrub()
        {

        }

        public void CollectGarbage()
        {

        }

        public IEnumerable<string> GetKeys()
        {
            return Directory.EnumerateFiles(this.GeneratePath(EntityStatus.Committed), "*", new EnumerationOptions() { RecurseSubdirectories = true });
        }

        public bool Remove(string key)
        {
            bool result = false;

            foreach (var entityStatus in new[] { EntityStatus.Temp, EntityStatus.Committed, EntityStatus.Backup })
            {
                var path = Path.Combine(this.GeneratePath(entityStatus), key);
                if (!Directory.Exists(key)) continue;

                Directory.Delete(path, true);
                result = true;
            }

            return result;
        }

        private enum EntityStatus
        {
            Temp,
            Committed,
            Backup,
        }

        private string GeneratePath(EntityStatus entityStatus)
        {
            return Path.Combine(_options.DirectoryPath, "Objects", entityStatus.ToString());
        }

        private static bool TryGet<T>(string basePath, string key, out T value) where T : IRocketPackObject<T>
        {
            value = default!;

            try
            {
                string directoryPath = Path.Combine(basePath, key);
                string contentPath = Path.Combine(directoryPath, "rpb.gz");
                string crcPath = Path.Combine(directoryPath, "crc");

                using var hub = new BytesHub();

                if (!File.Exists(contentPath) || !File.Exists(crcPath))
                {
                    return false;
                }

                using (var fileStream = new UnbufferedFileStream(contentPath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, BytesPool.Shared))
                using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    for (; ; )
                    {
                        var readLength = gzipStream.Read(hub.Writer.GetSpan(1024 * 4));
                        if (readLength <= 0)
                        {
                            break;
                        }

                        hub.Writer.Advance(readLength);
                    }
                }


                var sequence = hub.Reader.GetSequence();

                using (var fileStream = new UnbufferedFileStream(crcPath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, BytesPool.Shared))
                {
                    Span<byte> buffer = stackalloc byte[4];
                    fileStream.Read(buffer);

                    if (BinaryPrimitives.ReadInt32LittleEndian(buffer) != Crc32_Castagnoli.ComputeHash(sequence))
                    {
                        return false;
                    }
                }

                value = IRocketPackObject<T>.Import(sequence, BytesPool.Shared);

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        private static void Set<T>(string basePath, string key, T value) where T : IRocketPackObject<T>
        {
            try
            {
                string directoryPath = Path.Combine(basePath, key);
                string contentPath = Path.Combine(directoryPath, "rpb.gz");
                string crcPath = Path.Combine(directoryPath, "crc");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using var hub = new BytesHub();

                value.Export(hub.Writer, BytesPool.Shared);

                var sequence = hub.Reader.GetSequence();

                using (var fileStream = new UnbufferedFileStream(contentPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, BytesPool.Shared))
                using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Fastest))
                {
                    var position = sequence.Start;

                    while (sequence.TryGet(ref position, out var memory))
                    {
                        gzipStream.Write(memory.Span);
                    }
                }

                using (var fileStream = new UnbufferedFileStream(crcPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, BytesPool.Shared))
                {
                    Span<byte> buffer = stackalloc byte[4];
                    BinaryPrimitives.WriteInt32LittleEndian(buffer, Crc32_Castagnoli.ComputeHash(sequence));
                    fileStream.Write(buffer);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw e;
            }
        }

        private void Commit(string key)
        {
            var temp = Path.Combine(this.GeneratePath(EntityStatus.Temp), key);
            var committed = Path.Combine(this.GeneratePath(EntityStatus.Committed), key);
            var backup = Path.Combine(this.GeneratePath(EntityStatus.Backup), key);

            try
            {
                if (Directory.Exists(backup))
                {
                    Directory.Delete(backup, true);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw e;
            }

            try
            {
                if (Directory.Exists(committed))
                {
                    Directory.Move(committed, backup);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw e;
            }

            try
            {
                if (Directory.Exists(temp))
                {
                    Directory.Move(temp, committed);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw e;
            }
        }

        public bool TryGetStateState(out ObjectStoreState? state)
        {
            return this.TryGetContent<ObjectStoreState>("#State", out state);
        }

        public void SetStateState(ObjectStoreState state)
        {
            this.SetContent("#State", state);
        }

        public bool TryGetContent<T>(string key, out T value) where T : IRocketPackObject<T>
        {
            value = default!;

            foreach (var entityStatus in new[] { EntityStatus.Committed, EntityStatus.Backup })
            {
                if (TryGet<T>(this.GeneratePath(entityStatus), key, out var result))
                {
                    value = result;
                    return true;
                }
            }

            return false;
        }

        public void SetContent<T>(string key, T value) where T : IRocketPackObject<T>
        {
            Set(this.GeneratePath(EntityStatus.Temp), key, value);

            this.Commit(key);
        }
    }
}
