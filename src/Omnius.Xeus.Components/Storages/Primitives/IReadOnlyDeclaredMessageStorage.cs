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
    public interface IReadOnlyDeclaredMessageStorage
    {
        ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);
        ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default);
        ValueTask<bool> ContainsMessageAsync(OmniSignature signature, DateTime since = default);
        ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default);
        ValueTask<DeclaredMessage?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
