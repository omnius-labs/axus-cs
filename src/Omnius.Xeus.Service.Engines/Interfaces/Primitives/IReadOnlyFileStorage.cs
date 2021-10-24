using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines.Primitives
{
    public interface IReadOnlyFileStorage
    {
        ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<OmniHash>> GetRootHashesAsync(CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash rootHash, bool? exists = null, CancellationToken cancellationToken = default);

        ValueTask<bool> ContainsFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default);

        ValueTask<bool> ContainsBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);

        ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);
    }
}
