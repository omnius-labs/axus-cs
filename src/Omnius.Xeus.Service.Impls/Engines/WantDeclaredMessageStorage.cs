using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Service.Drivers;

namespace Omnius.Xeus.Service.Engines
{
    public sealed class WantDeclaredMessageStorage : AsyncDisposableBase, IWantDeclaredMessageStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantDeclaredMessageStorageOptions _options;
        private readonly IObjectStoreFactory _objectStoreFactory;
        private readonly IBytesPool _bytesPool;

        private readonly AsyncLock _asyncLock = new AsyncLock();

        internal sealed class WantDeclaredMessageStorageFactory : IWantDeclaredMessageStorageFactory
        {
            public async ValueTask<IWantDeclaredMessageStorage> CreateAsync(WantDeclaredMessageStorageOptions options, IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool)
            {
                var result = new WantDeclaredMessageStorage(options, objectStoreFactory, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IWantDeclaredMessageStorageFactory Factory { get; } = new WantDeclaredMessageStorageFactory();

        internal WantDeclaredMessageStorage(WantDeclaredMessageStorageOptions options, IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool)
        {
            _options = options;
            _objectStoreFactory = objectStoreFactory;
            _bytesPool = bytesPool;
        }

        internal async ValueTask InitAsync()
        {
            await this.StartWatching();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.StopWatching();
        }

        private async ValueTask StartWatching()
        {

        }

        private async ValueTask StopWatching()
        {

        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public ValueTask<WantDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ResourceTag> GetWantTags()
        {
            throw new NotImplementedException();
        }

        public bool Contains(OmniSignature signature, DateTime since = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<DeclaredMessage?> ReadAsync(OmniSignature signature, DateTime since = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask WriteAsync(DeclaredMessage message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask WantAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnwantAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
