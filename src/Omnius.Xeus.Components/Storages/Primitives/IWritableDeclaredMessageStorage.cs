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
    public interface IWritableDeclaredMessageStorage : IReadOnlyDeclaredMessageStorage
    {
        ValueTask WriteMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
    }
}
