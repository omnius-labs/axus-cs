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
    public interface IWritableMessageStorage : IReadOnlyMessageStorage
    {
        ValueTask SetDeclaredMessagesAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
        ValueTask SetOrientedMessagesAsync(OrientedMessage message, CancellationToken cancellationToken = default);
    }
}
