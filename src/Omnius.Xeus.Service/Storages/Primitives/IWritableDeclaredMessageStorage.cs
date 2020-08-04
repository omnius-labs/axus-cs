using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Engines;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Storages
{
    public interface IWritableDeclaredMessageStorage : IReadOnlyDeclaredMessageStorage
    {
        ValueTask WriteMessageAsync(DeclaredMessage message, CancellationToken cancellationToken = default);
    }
}
