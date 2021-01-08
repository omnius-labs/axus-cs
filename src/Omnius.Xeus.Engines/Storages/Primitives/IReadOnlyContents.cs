using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Engines.Storages.Primitives
{
    public interface IReadOnlyContents
    {
        ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<OmniHash>> GetContentHashesAsync(CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash contentHash, bool? exists = null, CancellationToken cancellationToken = default);

        ValueTask<bool> ContainsContentAsync(OmniHash contentHash);

        ValueTask<bool> ContainsBlockAsync(OmniHash contentHash, OmniHash blockHash);

        ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash contentHash, OmniHash blockHash, CancellationToken cancellationToken = default);
    }
}
