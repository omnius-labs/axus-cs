using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Drivers;

namespace Omnius.Xeus.Service.Engines
{
    public interface IWantMessageStorageFactory
    {
        ValueTask<IWantMessageStorage> CreateAsync(WantMessageStorageOptions options,
            IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool);
    }

    public interface IWantMessageStorage : IWantStorage, IWritableMessageStorage
    {
        ValueTask<WantMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask WantDeclaredMessageAsync(OmniHash hash, CancellationToken cancellationToken = default);
        ValueTask UnwantDeclaredMessageAsync(OmniHash hash, CancellationToken cancellationToken = default);
        ValueTask WantOrientedMessageAsync(OmniHash hash, CancellationToken cancellationToken = default);
        ValueTask UnwantOrientedMessageAsync(OmniHash hash, CancellationToken cancellationToken = default);
    }
}
