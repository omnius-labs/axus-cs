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

        ValueTask<IEnumerable<OmniHash>> GetRootHashesAsync(CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash rootHash, bool? exists = null, CancellationToken cancellationToken = default);

        ValueTask<bool> ContainsContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

        ValueTask<bool> ContainsBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);

        ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);
    }
}
