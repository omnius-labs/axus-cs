using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IPublishFileStorageFactory
    {
        ValueTask<IPublishFileStorage> CreateAsync(string configPath, IBytesPool bytesPool);
    }

    public interface IPublishFileStorage : IPublishStorage, IAsyncDisposable
    {
        ValueTask<OmniHash> AddPublishFileAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask RemovePublishFileAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
