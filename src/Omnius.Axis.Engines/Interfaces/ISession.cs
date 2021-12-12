using Omnius.Axis.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;

namespace Omnius.Axis.Engines;

public interface ISession : IAsyncDisposable
{
    IConnection Connection { get; }

    OmniAddress Address { get; }

    SessionHandshakeType HandshakeType { get; }

    OmniSignature Signature { get; }

    string Scheme { get; }
}
