using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Storages;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Interactors
{
    public interface IUserProfileUploaderFactory
    {
        ValueTask<IUserProfileUploader> CreateAsync(UserProfileUploaderOptions options, IXeusService xeusService,
            IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface IUserProfileUploader : IAsyncDisposable
    {
        ValueTask<IEnumerable<UploadingUserProfileReport>> GetUploadingUserProfileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask RegisterAsync(XeusUserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default);

        ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
