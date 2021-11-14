using Omnius.Core.Cryptography;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines;

public interface ISession
{
    IConnection Connection { get; }

    OmniAddress Address { get; }

    SessionHandshakeType HandshakeType { get; }

    OmniSignature Signature { get; }

    string Scheme { get; }
}
