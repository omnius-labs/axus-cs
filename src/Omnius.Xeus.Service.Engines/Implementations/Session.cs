using Omnius.Core.Cryptography;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines;

public record Session : ISession
{
    public Session(IConnection connection, OmniAddress address, SessionHandshakeType handshakeType, OmniSignature signature, string scheme)
    {
        this.Connection = connection;
        this.Address = address;
        this.HandshakeType = handshakeType;
        this.Signature = signature;
        this.Scheme = scheme;
    }

    public IConnection Connection { get; }

    public OmniAddress Address { get; }

    public SessionHandshakeType HandshakeType { get; }

    public OmniSignature Signature { get; }

    public string Scheme { get; }
}