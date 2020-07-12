using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Engines;

namespace Omnius.Xeus.Service.Engines
{
    public interface IReadOnlyMessageStorage
    {
        ValueTask<DeclaredMessage[]> GetDeclaredMessagesAsync(OmniHash key, CancellationToken cancellationToken = default);
        ValueTask<OrientedMessage[]> GetOrientedMessagesAsync(OmniHash key, CancellationToken cancellationToken = default);
    }
}
