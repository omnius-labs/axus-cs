using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;
using Omnius.Xeus.Interactors.Storages.Internal.Repositories;

namespace Omnius.Xeus.Interactors.Storages
{
    public sealed class UserProfileStorage : AsyncDisposableBase, IUserProfileStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly UserProfileStorageOptions _options;
        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        private readonly UserProfileStorageRepository _repository;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        internal sealed class UserProfileStorageFactory : IUserProfileStorageFactory
        {
            public async ValueTask<IUserProfileStorage> CreateAsync(UserProfileStorageOptions options, IXeusService xeusService, IBytesPool bytesPool)
            {
                var result = new UserProfileStorage(options, xeusService, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public UserProfileStorage(UserProfileStorageOptions options, IXeusService xeusService, IBytesPool bytesPool)
        {
            _options = options;
            _xeusService = xeusService;
            _bytesPool = bytesPool;

            _repository = new UserProfileStorageRepository(Path.Combine(options.ConfigDirectoryPath, "database"));
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
