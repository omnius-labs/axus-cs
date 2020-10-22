using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Api;

namespace Omnius.Xeus.Interactors.FileStorage
{
    public sealed class FileStorageService : AsyncDisposableBase, IFileStorageService
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // private readonly FileStorageServiceOptions _options;
        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        internal sealed class FileStorageServiceFactory : IFileStorageServiceFactory
        {
            public async ValueTask<IFileStorageService> CreateAsync(IXeusService xeusService)
            {
                throw new NotImplementedException();
            }
        }

        public FileStorageService()
        {
        }

        protected override ValueTask OnDisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
