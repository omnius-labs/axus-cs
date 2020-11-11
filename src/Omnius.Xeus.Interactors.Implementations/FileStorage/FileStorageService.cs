using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.FileStorage
{
    public sealed class FileStorageService : AsyncDisposableBase, IFileStorageService
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly FileStorageServiceOptions _options;
        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        internal sealed class FileStorageServiceFactory : IFileStorageServiceFactory
        {
            public async ValueTask<IFileStorageService> CreateAsync(FileStorageServiceOptions options, IXeusService xeusService, IBytesPool bytesPool)
            {
                var result = new FileStorageService(options, xeusService, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public FileStorageService(FileStorageServiceOptions options, IXeusService xeusService, IBytesPool bytesPool)
        {
            _options = options;
            _xeusService = xeusService;
            _bytesPool = bytesPool;
        }

        public async ValueTask InitAsync()
        {
        }

        protected override ValueTask OnDisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
