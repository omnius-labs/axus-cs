using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Storages;
using Omnius.Xeus.Daemon;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
{
    public interface IUserProfileUploaderFactory
    {
        ValueTask<IUserProfileUploader> CreateAsync(UserProfileUploaderOptions options, IXeusService xeusService,
            IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IUserProfileUploader : IAsyncDisposable
    {
        ValueTask<IEnumerable<UploadingUserProfileReport>> GetUploadingUserProfileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask RegisterAsync(UserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default);

        ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
