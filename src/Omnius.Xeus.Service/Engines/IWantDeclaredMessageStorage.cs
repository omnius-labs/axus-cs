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

    public interface IWantDeclaredMessageStorage : IWritableDeclaredMessageStorage
    {
        ValueTask<WantDeclaredMessageStorageReport> GetReportAsync(CancellationToken cancellationToken = default);
        ValueTask AddAsync(OmniSignature signature, CancellationToken cancellationToken = default);
        ValueTask RemoveAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
