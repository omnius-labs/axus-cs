using Omnius.Axus.Core.Engine;
using Omnius.Axus.Core.Engine.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Secure;
using Omnius.Core.Net.Connections.Secure.V1;

namespace Omnius.Axus.Core.Engine.Services;

public sealed class SessionConnector : AsyncDisposableBase, ISessionConnector
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IEnumerable<IConnectionConnector> _connectionConnectors;
    private readonly IBytesPool _bytesPool;
    private readonly SessionConnectorOptions _options;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private const int MaxReceiveByteCount = 1024 * 1024 * 256;

    public static async ValueTask<SessionConnector> CreateAsync(IEnumerable<IConnectionConnector> connectionConnectors, IBytesPool bytesPool, SessionConnectorOptions options, CancellationToken cancellationToken = default)
    {
        var sessionConnector = new SessionConnector(connectionConnectors, bytesPool, options);
        return sessionConnector;
    }

    private SessionConnector(IEnumerable<IConnectionConnector> connectionConnectors, IBytesPool bytesPool, SessionConnectorOptions options)
    {
        _connectionConnectors = connectionConnectors;
        _bytesPool = bytesPool;
        _options = options;
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    public async ValueTask<ISession?> ConnectAsync(OmniAddress address, string scheme, CancellationToken cancellationToken = default)
    {
        foreach (var connector in _connectionConnectors)
        {
            var disposableList = new List<IAsyncDisposable>();

            try
            {
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedTokenSource.CancelAfter(TimeSpan.FromMinutes(2));

                var connection = await connector.ConnectAsync(address, linkedTokenSource.Token);
                if (connection is null) continue;
                disposableList.Add(connection);

                var session = await this.CreateSessionAsync(connection, address, scheme, linkedTokenSource.Token);
                disposableList.Add(session);

                return session;
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e, "Operation Canceled");

                foreach (var item in disposableList)
                {
                    await item.DisposeAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected Exception");

                foreach (var item in disposableList)
                {
                    await item.DisposeAsync();
                }
            }
        }

        return null;
    }

    private async ValueTask<ISession> CreateSessionAsync(IConnection connection, OmniAddress address, string scheme, CancellationToken cancellationToken)
    {
        var sendHelloMessage = new SessionManagerHelloMessage(new[] { SessionManagerVersion.Version1 });
        var receiveHelloMessage = await connection.ExchangeAsync(sendHelloMessage, cancellationToken);

        var version = EnumHelper.GetOverlappedMaxValue(sendHelloMessage.Versions, receiveHelloMessage.Versions);

        if (version == SessionManagerVersion.Version1)
        {
            var secureConnectionOptions = new OmniSecureConnectionOptions(OmniSecureConnectionType.Connected, _options.DigitalSignature, MaxReceiveByteCount);
            var secureConnection = OmniSecureConnection.CreateV1(connection, _bytesPool, secureConnectionOptions);

            await secureConnection.HandshakeAsync(cancellationToken);
            if (secureConnection.Signature is null) throw new Exception("Signature is null");

            // 自分自身の場合は接続しない
            if (secureConnection.Signature == _options.DigitalSignature.GetOmniSignature()) throw new Exception("Signature is same as myself");

            var sessionRequestMessage = new SessionManagerSessionRequestMessage(scheme);
            var sessionResultMessage = await secureConnection.SendAndReceiveAsync<SessionManagerSessionRequestMessage, SessionManagerSessionResultMessage>(sessionRequestMessage, cancellationToken);

            if (sessionResultMessage.Type != SessionManagerSessionResultType.Accepted) throw new Exception("Session is not accepted");

            var session = new Session(secureConnection, address, SessionHandshakeType.Connected, secureConnection.Signature, scheme);
            return session;
        }

        throw new NotSupportedException();
    }
}
