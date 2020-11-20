using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;
using Omnius.Xeus.Interactors.Storages.Internal.Repositories;

namespace Omnius.Xeus.Interactors.Storages
{
    public sealed class FileStorage : AsyncDisposableBase, IFileStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly FileStorageOptions _options;
        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        private readonly FileStorageRepository _repository;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        internal sealed class FileStorageFactory : IFileStorageFactory
        {
            public async ValueTask<IFileStorage> CreateAsync(FileStorageOptions options, IXeusService xeusService, IBytesPool bytesPool)
            {
                var result = new FileStorage(options, xeusService, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public FileStorage(FileStorageOptions options, IXeusService xeusService, IBytesPool bytesPool)
        {
            _options = options;
            _xeusService = xeusService;
            _bytesPool = bytesPool;

            _repository = new FileStorageRepository(_options.ConfigDirectoryPath);
        }

        public async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _repository.Dispose();
        }

        public void RegisterSearchSignatures(IEnumerable<OmniSignature> signatures)
        {
            foreach (var s in signatures)
            {
                _repository.SubscribedSignatures.Add(s);
            }
        }

        public void UnregisterSearchSignatures(IEnumerable<OmniSignature> signatures)
        {
            foreach (var s in signatures)
            {
                _repository.SubscribedSignatures.Remove(s);
            }
        }
    }
}
