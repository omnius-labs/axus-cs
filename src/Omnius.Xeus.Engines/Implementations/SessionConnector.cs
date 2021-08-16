using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Secure;
using Omnius.Core.Net.Connections.Secure.V1;
using Omnius.Xeus.Engines.Internal.Models;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public sealed class SessionConnector : AsyncDisposableBase, ISessionConnector
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SessionConnectorOptions _options;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        internal SessionConnector(SessionConnectorOptions options)
        {
            _options = options;
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async ValueTask<ISession?> ConnectAsync(OmniAddress address, string scheme, CancellationToken cancellationToken = default)
        {
            foreach (var connector in _options.Connectors ?? Enumerable.Empty<IConnectionConnector>())
            {
                var connection = await connector.ConnectAsync(address, cancellationToken);
                if (connection is null) continue;

                var session = await this.CreateSessionAsync(connection, address, scheme, cancellationToken);
                return session;
            }

            return null;
        }

        private async ValueTask<ISession> CreateSessionAsync(IConnection connection, OmniAddress address, string scheme, CancellationToken cancellationToken)
        {
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(20));

            var sendHelloMessage = new SessionManagerHelloMessage(new[] { SessionManagerVersion.Version1 });
            var receiveHelloMessage = await connection.ExchangeAsync(sendHelloMessage, cancellationToken);

            var version = EnumHelper.GetOverlappedMaxValue(sendHelloMessage.Versions, receiveHelloMessage.Versions);

            if (version == SessionManagerVersion.Version1)
            {
                var secureConnectionOptions = new OmniSecureConnectionOptions()
                {
                    Type = OmniSecureConnectionType.Connected,
                    DigitalSignature = _options.DigitalSignature,
                    BatchActionDispatcher = _options.BatchActionDispatcher,
                    BytesPool = _options.BytesPool,
                };
                var secureConnection = new OmniSecureConnection(connection, secureConnectionOptions);

                await secureConnection.HandshakeAsync(linkedTokenSource.Token);

                var sessionRequestMessage = new SessionManagerSessionRequestMessage(scheme);
                var sessionResultMessage = secureConnection.SendAndReceiveAsync<SessionManagerSessionRequestMessage, SessionManagerSessionResultMessage>(sessionRequestMessage, linkedTokenSource.Token);

                var session = new Session(secureConnection, address, SessionHandshakeType.Connected, secureConnection.Signature, scheme);
                return session;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
