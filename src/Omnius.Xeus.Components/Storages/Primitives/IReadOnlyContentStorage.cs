using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Components.Engines;
using Omnius.Xeus.Components.Models;

namespace Omnius.Xeus.Components.Storages
{
    public interface IReadOnlyContentStorage
    {
        ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);
        ValueTask<IEnumerable<OmniHash>> GetContentHashesAsync(CancellationToken cancellationToken = default);
        ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask<bool> ContainsContentAsync(OmniHash rootHash);
        ValueTask<bool> ContainsBlockAsync(OmniHash rootHash, OmniHash targetHash);
        ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default);
    }
}
