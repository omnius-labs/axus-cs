using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Secure;
using Omnius.Core.Net.Connections.Secure.V1;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Xeus.Engines.Internal.Models;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public sealed partial class SessionAccepter : AsyncDisposableBase, ISessionAccepter
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SessionAccepterOptions _options;

        private readonly SessionChannels _sessionChannels = new(3);

        private readonly Task _acceptLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        internal SessionAccepter(SessionAccepterOptions options)
        {
            _options = options;

            _acceptLoopTask = this.InternalAcceptLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await _acceptLoopTask;

            _cancellationTokenSource.Dispose();
        }

        public async ValueTask<ISession> AcceptAsync(string scheme, CancellationToken cancellationToken = default)
        {
            var channel = _sessionChannels.GetOrCreate(scheme);
            return await channel!.Reader.ReadAsync(cancellationToken);
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
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw;
            }
        }

        private async ValueTask<ISession?> InternalAcceptAsync(CancellationToken cancellationToken = default)
        {
            foreach (var accepter in _options.Accepters ?? Enumerable.Empty<IConnectionAccepter>())
            {
                var acceptedResult = await accepter.AcceptAsync(cancellationToken);
                if (acceptedResult is null) continue;

                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(20));

                var session = await this.CreateSessionAsync(acceptedResult.Connection, acceptedResult.Address, linkedTokenSource.Token);
                if (session is null) continue;
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
                var secureConnectionOptions = new OmniSecureConnectionOptions()
                {
                    Type = OmniSecureConnectionType.Accepted,
                    DigitalSignature = _options.DigitalSignature,
                    BatchActionDispatcher = _options.BatchActionDispatcher,
                    BytesPool = _options.BytesPool,
                };
                var secureConnection = new OmniSecureConnection(connection, secureConnectionOptions);

                await secureConnection.HandshakeAsync(cancellationToken);

                var receivedMessage = await connection.Receiver.ReceiveAsync<SessionManagerSessionRequestMessage>(cancellationToken);
                var sendingMessage = new SessionManagerSessionResultMessage(_sessionChannels.Contains(receivedMessage.Scheme) ? SessionManagerSessionResultType.Accepted : SessionManagerSessionResultType.Rejected);

                await connection.Sender.SendAsync(sendingMessage, cancellationToken);

                var session = new Session(secureConnection, address, SessionHandshakeType.Accepted, secureConnection.Signature, receivedMessage.Scheme);
                return session;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public async ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<OmniAddress>();

            foreach (var accepter in _options.Accepters ?? Enumerable.Empty<IConnectionAccepter>())
            {
                results.AddRange(await accepter.GetListenEndpointsAsync(cancellationToken));
            }

            return results.ToArray();
        }
    }
}
