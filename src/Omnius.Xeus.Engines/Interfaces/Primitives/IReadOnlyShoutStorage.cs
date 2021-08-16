using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines.Primitives
{
    public interface IReadOnlyShoutStorage
    {
        ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default);

        ValueTask<bool> ContainsShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask<DateTime?> ReadShoutCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask<Shout?> ReadShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
