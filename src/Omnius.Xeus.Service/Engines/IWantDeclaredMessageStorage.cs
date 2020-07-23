using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public interface IWantDeclaredMessageStorageFactory
    {
        ValueTask<IWantDeclaredMessageStorage> CreateAsync(WantDeclaredMessageStorageOptions options, IBytesPool bytesPool);
    }

    public interface IWantDeclaredMessageStorage : IWantStorage, IWritableDeclaredMessageStorage
    {
        ValueTask<WantDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask WantAsync(OmniSignature signature, CancellationToken cancellationToken = default);
        ValueTask UnwantAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
