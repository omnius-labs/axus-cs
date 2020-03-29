using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Connections.Secure;
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines.Internal;

namespace Omnius.Xeus.Service.Engines
{
    public sealed class NodeExplorer : AsyncDisposableBase, INodeExplorer
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly NodeExplorerOptions _options;
        private readonly IObjectStoreFactory _objectStoreFactory;
        private readonly IConnectionController _connectionController;
        private readonly IPublishStorage _publishStorage;
        private readonly IWantStorage _wantStorage;
        private readonly IBytesPool _bytesPool;

        private readonly ReadOnlyMemory<byte> _myId;
        private IObjectStore _objectStore;

        private readonly HashSet<ConnectionStatus> _connections = new HashSet<ConnectionStatus>();
        private readonly HashSet<NodeProfile> _nodeProfileSet = new HashSet<NodeProfile>();

        private Task _connectLoopTask;
        private Task _acceptLoopTask;
        private Task _sendLoopTask;
        private Task _receiveLoopTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private object _lockObject = new object();

        private const string ServiceType = "NodeExplorer`@v1";

        internal sealed class NodeExplorerFactory : INodeExplorerFactory
        {
            public async ValueTask<INodeExplorer> CreateAsync(string configPath, NodeExplorerOptions options,
                IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
                IPublishStorage publishStorage, IWantStorage wantStorage, IBytesPool bytesPool)
            {
                var result = new NodeExplorer(configPath, options, objectStoreFactory, connectionController, publishStorage, wantStorage, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static INodeExplorerFactory Factory { get; } = new NodeExplorerFactory();

        internal NodeExplorer(string configPath, NodeExplorerOptions options,
                IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
                IPublishStorage publishStorage, IWantStorage wantStorage, IBytesPool bytesPool)
        {
            _configPath = configPath;
            _options = options;
            _objectStoreFactory = objectStoreFactory;
            _connectionController = connectionController;
            _publishStorage = publishStorage;
            _wantStorage = wantStorage;
            _bytesPool = bytesPool;

            {
                var id = new byte[32];
                using var random = RandomNumberGenerator.Create();
                random.GetBytes(id);
                _myId = id;
            }
        }

        public async ValueTask InitAsync()
        {
            _objectStore = await _objectStoreFactory.CreateAsync(_configPath, _bytesPool);

            await this.LoadAsync();

            _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
            _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
            _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
            _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_connectLoopTask, _acceptLoopTask, _sendLoopTask, _receiveLoopTask);

            await this.SaveAsync();

            _cancellationTokenSource.Dispose();
            await _objectStore.DisposeAsync();
        }

        private async ValueTask LoadAsync()
        {

        }

        private async ValueTask SaveAsync()
        {

        }

        private async ValueTask AddConnectionAsync(IConnection connection, OmniAddress address, ConnectionHandshakeType handshakeType, CancellationToken cancellationToken = default)
        {
            var status = new ConnectionStatus(connection, address, handshakeType);

            var myNodeProflie = new NodeProfile(await _connectionController.GetListenEndpointsAsync(cancellationToken));

            var myHelloMessage = new NodeExplorerHelloMessage(_myId, myNodeProflie);
            NodeExplorerHelloMessage? otherHelloMessage = null;

            var enqueueTask = connection.EnqueueAsync((bufferWriter) => myNodeProflie.Export(bufferWriter, _bytesPool), cancellationToken);
            var dequeueTask = connection.DequeueAsync((sequence) => otherHelloMessage = NodeExplorerHelloMessage.Import(sequence, _bytesPool), cancellationToken);

            await ValueTaskHelper.WhenAll(enqueueTask, dequeueTask);
            if (otherHelloMessage == null) return;

            status.Id = otherHelloMessage.Id;
            status.NodeProfile = otherHelloMessage.NodeProfile;

            lock (_lockObject)
            {
                _connections.Add(status);
            }
        }

        private async Task ConnectLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);

                    lock (_lockObject)
                    {
                        int connectionCount = _connections.Select(n => n.HandshakeType == ConnectionHandshakeType.Connected).Count();

                        if (_connections.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    OmniAddress? targetAddress = null;

                    lock (_lockObject)
                    {
                        var nodeProfiles = _nodeProfileSet.ToArray();
                        random.Shuffle(nodeProfiles);

                        var ignoreAddressSet = new HashSet<OmniAddress>(_connections.Select(n => n.Address));

                        foreach (var address in nodeProfiles.SelectMany(n => n.Addresses))
                        {
                            if (ignoreAddressSet.Contains(address)) continue;
                            targetAddress = address;
                            break;
                        }
                    }

                    if (targetAddress == null) continue;

                    var connection = await _connectionController.ConnectAsync(targetAddress, ServiceType, cancellationToken);
                    if (connection != null)
                    {
                        await this.AddConnectionAsync(connection, targetAddress, ConnectionHandshakeType.Connected);
                    }
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

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);

                    lock (_lockObject)
                    {
                        int connectionCount = _connections.Select(n => n.HandshakeType == ConnectionHandshakeType.Accepted).Count();

                        if (_connections.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    var result = await _connectionController.AcceptAsync(ServiceType, cancellationToken);
                    if (result.Connection != null && result.Address != null)
                    {
                        await this.AddConnectionAsync(result.Connection, result.Address, ConnectionHandshakeType.Accepted);
                    }
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

        private enum MessageType
        {
            PushNodeProfiles = 0,
            PushContentLocations = 1,
            WantContentLocations = 2,
            GiveContentLocations = 3,
        }

        private async Task SendLoopAsync(CancellationToken cancellationToken )
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();


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

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();
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

        private enum ConnectionHandshakeType
        {
            Connected,
            Accepted,
        }

        private class ConnectionStatus : ISynchronized
        {
            public ConnectionStatus(IConnection connection, OmniAddress address, ConnectionHandshakeType handshakeType)
            {
                this.Connection = connection;
                this.Address = address;
                this.HandshakeType = handshakeType;
            }

            public object LockObject { get; } = new object();

            public IConnection Connection { get; }
            public OmniAddress Address { get; }
            public ConnectionHandshakeType HandshakeType { get; }

            public NodeProfile NodeProfile { get; set; }
            public ReadOnlyMemory<byte> Id { get; set; }
        }
    }
}
