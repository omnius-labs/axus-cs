using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Connections.Secure;
using Omnius.Xeus.Service.Drivers.Internal;

namespace Omnius.Xeus.Service.Drivers
{
    public sealed class ConnectionController : AsyncDisposableBase, IConnectionController
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ConnectionControllerOptions _options;
        private readonly IBytesPool _bytesPool;

        private TcpConnector _tcpConnector;
        private BaseConnectionDispatcher _baseConnectionDispatcher;

        private readonly ConcurrentDictionary<string, Channel<ConnectionControllerAcceptResult>> _acceptedConnectionChannels = new ConcurrentDictionary<string, Channel<ConnectionControllerAcceptResult>>();

        private Task _acceptLoopTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        internal sealed class ConnectionControllerFactory : IConnectionControllerFactory
        {
            public async ValueTask<IConnectionController> CreateAsync(ConnectionControllerOptions options, IBytesPool bytesPool)
            {
                var result = new ConnectionController(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IConnectionControllerFactory Factory { get; } = new ConnectionControllerFactory();

        internal ConnectionController(ConnectionControllerOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;
        }

        public async ValueTask InitAsync()
        {
            _tcpConnector = await TcpConnector.Factory.CreateAsync(_options.TcpConnectOptions, _options.TcpAcceptOptions, _bytesPool);
            _baseConnectionDispatcher = new BaseConnectionDispatcher(new BaseConnectionDispatcherOptions()
            {
                MaxSendBytesPerSeconds = (int)_options.BandwidthOptions.MaxSendBytesPerSeconds,
                MaxReceiveBytesPerSeconds = (int)_options.BandwidthOptions.MaxReceiveBytesPerSeconds,
            });

            _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await _acceptLoopTask;

            _cancellationTokenSource.Dispose();

            await _tcpConnector.DisposeAsync();
            await _baseConnectionDispatcher.DisposeAsync();
        }

        private Channel<ConnectionControllerAcceptResult>? GetAcceptedConnectionChannel(string serviceName, bool createIfNotFound)
        {
            if (createIfNotFound)
            {
                return _acceptedConnectionChannels.GetOrAdd(serviceName, (_) => Channel.CreateBounded<ConnectionControllerAcceptResult>(new BoundedChannelOptions(3) { FullMode = BoundedChannelFullMode.Wait }));
            }
            else
            {
                _acceptedConnectionChannels.TryGetValue(serviceName, out var result);
                return result;
            }
        }

        private async ValueTask<IConnection?> InternalConnectAsync(ICap cap, string serviceName, CancellationToken cancellationToken)
        {
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(20));

            var baseConnectionOptions = new BaseConnectionOptions()
            {
                MaxSendByteCount = 4 * 1024 * 1024,
                MaxReceiveByteCount = 4 * 1024 * 1024,
                BytesPool = _bytesPool,
            };
            var baseConnection = new BaseConnection(cap, _baseConnectionDispatcher, baseConnectionOptions);

            var omniSecureConnectionOptions = new OmniSecureConnectionOptions()
            {
                Type = OmniSecureConnectionType.Connected,
                BufferPool = _bytesPool,
            };
            var omniSecureConnection = new OmniSecureConnection(baseConnection, omniSecureConnectionOptions);

            await omniSecureConnection.HandshakeAsync(linkedTokenSource.Token);

            var helloMessage = new ConnectionHelloMessage(serviceName);
            await omniSecureConnection.EnqueueAsync((bufferWriter) => helloMessage.Export(bufferWriter, _bytesPool), linkedTokenSource.Token);

            return omniSecureConnection;
        }

        private async ValueTask<(IConnection?, string?)> InternalAcceptAsync(ICap cap, CancellationToken cancellationToken)
        {
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(20));

            var baseConnectionOptions = new BaseConnectionOptions()
            {
                MaxSendByteCount = 4 * 1024 * 1024,
                MaxReceiveByteCount = 4 * 1024 * 1024,
                BytesPool = _bytesPool,
            };
            var baseConnection = new BaseConnection(cap, _baseConnectionDispatcher, baseConnectionOptions);

            var omniSecureConnectionOptions = new OmniSecureConnectionOptions()
            {
                Type = OmniSecureConnectionType.Accepted,
                BufferPool = _bytesPool,
            };
            var omniSecureConnection = new OmniSecureConnection(baseConnection, omniSecureConnectionOptions);

            await omniSecureConnection.HandshakeAsync(linkedTokenSource.Token);

            ConnectionHelloMessage? helloMessage = null;
            await omniSecureConnection.DequeueAsync((sequence) => helloMessage = ConnectionHelloMessage.Import(sequence, _bytesPool), linkedTokenSource.Token);

            return (omniSecureConnection, helloMessage?.Service);
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);

                    var (cap, address) = await _tcpConnector.AcceptAsync(cancellationToken);
                    if (cap == null || address == null) continue;

                    var (connection, serviceName) = await this.InternalAcceptAsync(cap, cancellationToken);
                    if (connection == null || serviceName == null) continue;

                    var channel = this.GetAcceptedConnectionChannel(serviceName, false);
                    if (channel == null)
                    {
                        connection.Dispose();
                        cap.Dispose();
                        continue;
                    }

                    var result = new ConnectionControllerAcceptResult(connection, address);
                    await channel.Writer.WriteAsync(result);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        public async ValueTask<IConnection?> ConnectAsync(OmniAddress address, string serviceName, CancellationToken cancellationToken = default)
        {
            var cap = await _tcpConnector.ConnectAsync(address, cancellationToken);

            if (cap != null)
            {
                var connection = await this.InternalConnectAsync(cap, serviceName, cancellationToken);
                return connection;
            }

            return null;
        }

        public async ValueTask<ConnectionControllerAcceptResult> AcceptAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            var channel = this.GetAcceptedConnectionChannel(serviceName, true);
            return await channel!.Reader.ReadAsync(cancellationToken);
        }

        public async ValueTask<OmniAddress[]> GetListenEndpointsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var results = new List<OmniAddress>();
            results.AddRange(await _tcpConnector.GetListenEndpointsAsync(cancellationToken));

            return results.ToArray();
        }
    }
}
