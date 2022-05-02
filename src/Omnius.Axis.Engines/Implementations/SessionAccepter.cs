using System.Collections.Immutable;
using Omnius.Axis.Engines.Internal.Models;
using Omnius.Axis.Models;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Secure;
using Omnius.Core.Net.Connections.Secure.V1;
using Omnius.Core.Tasks;

namespace Omnius.Axis.Engines;

public sealed partial class SessionAccepter : AsyncDisposableBase, ISessionAccepter
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly ImmutableArray<IConnectionAccepter> _connectionAccepters;
    private readonly IBatchActionDispatcher _batchActionDispatcher;
    private readonly IBytesPool _bytesPool;
    private readonly SessionAccepterOptions _options;

    private readonly SessionChannels _sessionChannels = new(3);

    private readonly Task _acceptLoopTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private const int MaxReceiveByteCount = 1024 * 1024 * 256;

    public static async ValueTask<SessionAccepter> CreateAsync(IEnumerable<IConnectionAccepter> connectionAccepters, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, SessionAccepterOptions options, CancellationToken cancellationToken = default)
    {
        var sessionAccepter = new SessionAccepter(connectionAccepters, batchActionDispatcher, bytesPool, options);
        return sessionAccepter;
    }

    private SessionAccepter(IEnumerable<IConnectionAccepter> connectionAccepters, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, SessionAccepterOptions options)
    {
        _connectionAccepters = connectionAccepters.ToImmutableArray();
        _batchActionDispatcher = batchActionDispatcher;
        _bytesPool = bytesPool;
        _options = options;

        _acceptLoopTask = this.InternalAcceptLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();

        await _acceptLoopTask;

        _cancellationTokenSource.Dispose();
    }

    private async Task InternalAcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var random = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);

                var session = await this.InternalAcceptAsync(cancellationToken);
                if (session is null) continue;

                if (!_sessionChannels.TryGet(session.Scheme, out var channel))
                {
                    await session.Connection.DisposeAsync();
                    continue;
                }

                await channel.Writer.WriteAsync(session, cancellationToken);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask<ISession?> InternalAcceptAsync(CancellationToken cancellationToken = default)
    {
        foreach (var accepter in _connectionAccepters)
        {
            var acceptedResult = await accepter.AcceptAsync(cancellationToken);
            if (acceptedResult is null) continue;

            try
            {
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedTokenSource.CancelAfter(TimeSpan.FromMinutes(2));

                var session = await this.CreateSessionAsync(acceptedResult.Connection, acceptedResult.Address, linkedTokenSource.Token);
                return session;
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e, "Operation Canceled");

                await acceptedResult.Connection.DisposeAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected Exception");

                await acceptedResult.Connection.DisposeAsync();
            }
        }

        return null;
    }

    private async ValueTask<ISession> CreateSessionAsync(IConnection connection, OmniAddress address, CancellationToken cancellationToken)
    {
        var sendHelloMessage = new SessionManagerHelloMessage(new[] { SessionManagerVersion.Version1 });
        var receiveHelloMessage = await connection.ExchangeAsync(sendHelloMessage, cancellationToken);

        var version = EnumHelper.GetOverlappedMaxValue(sendHelloMessage.Versions, receiveHelloMessage.Versions);

        if (version == SessionManagerVersion.Version1)
        {
            var secureConnectionOptions = new OmniSecureConnectionOptions(OmniSecureConnectionType.Accepted, _options.DigitalSignature, MaxReceiveByteCount);
            var secureConnection = OmniSecureConnection.CreateV1(connection, _bytesPool, secureConnectionOptions);

            await secureConnection.HandshakeAsync(cancellationToken);
            if (secureConnection.Signature is null) throw new Exception("Signature is null");

            // 自分自身の場合は接続しない
            if (secureConnection.Signature == _options.DigitalSignature.GetOmniSignature()) throw new Exception("Signature is same as myself");

            var receivedMessage = await secureConnection.Receiver.ReceiveAsync<SessionManagerSessionRequestMessage>(cancellationToken);
            var sendingMessage = new SessionManagerSessionResultMessage(_sessionChannels.Contains(receivedMessage.Scheme) ? SessionManagerSessionResultType.Accepted : SessionManagerSessionResultType.Rejected);
            await secureConnection.Sender.SendAsync(sendingMessage, cancellationToken);

            if (sendingMessage.Type != SessionManagerSessionResultType.Accepted) throw new Exception("Session is not accepted");

            var session = new Session(secureConnection, address, SessionHandshakeType.Accepted, secureConnection.Signature, receivedMessage.Scheme);
            return session;
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public async ValueTask<ISession> AcceptAsync(string scheme, CancellationToken cancellationToken = default)
    {
        var channel = _sessionChannels.GetOrCreate(scheme);
        return await channel!.Reader.ReadAsync(cancellationToken);
    }

    public async ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<OmniAddress>();

        foreach (var connectionAccepter in _connectionAccepters)
        {
            results.AddRange(await connectionAccepter.GetListenEndpointsAsync(cancellationToken));
        }

        return results.ToArray();
    }
}
