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

    public interface IWantMessageStorage : IWantStorage
    {
        ValueTask<WantMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask WantMessageAsync(Tag tag, CancellationToken cancellationToken = default);
        ValueTask UnwantMessageAsync(Tag tag, CancellationToken cancellationToken = default);
        ValueTask<DeclaredMessage[]> ExportDeclaredMessagesAsync(Tag tag, CancellationToken cancellationToken = default);
        ValueTask<OrientedMessage[]> ExportOrientedMessagesAsync(Tag tag, CancellationToken cancellationToken = default);
    }
}
