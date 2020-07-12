using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Extensions;
using Omnius.Core.Helpers;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines.Internal;

namespace Omnius.Xeus.Service.Engines
{
    public sealed class DeclaredMessageExchanger : AsyncDisposableBase, IDeclaredMessageExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly DeclaredMessageExchangerOptions _options;
        private readonly IObjectStoreFactory _objectStoreFactory;
        private readonly IConnectionController _connectionController;
        private readonly INodeFinder _nodeFinder;
        private readonly IPublishMessageStorage _publishStorage;
        private readonly IWantMessageStorage _wantStorage;
        private readonly IBytesPool _bytesPool;

        private readonly ReadOnlyMemory<byte> _myId;
        private IObjectStore _objectStore;

        private readonly HashSet<ConnectionStatus> _connectionStatusSet = new HashSet<ConnectionStatus>();
        private readonly LinkedList<NodeProfile> _cloudNodeProfiles = new LinkedList<NodeProfile>();

        private Task _connectLoopTask;
        private Task _acceptLoopTask;
        private Task _sendLoopTask;
        private Task _receiveLoopTask;
        private Task _computeLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly object _lockObject = new object();

        public const string ServiceName = "message-exchanger`";

        internal sealed class DeclaredMessageExchangerFactory : IDeclaredMessageExchangerFactory
        {
            public async ValueTask<IDeclaredMessageExchanger> CreateAsync(DeclaredMessageExchangerOptions options, IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
                INodeFinder nodeFinder, IPublishMessageStorage publishStorage, IWantMessageStorage wantStorage, IBytesPool bytesPool)
            {
                var result = new DeclaredMessageExchanger(options, objectStoreFactory, connectionController, nodeFinder, publishStorage, wantStorage, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IDeclaredMessageExchangerFactory Factory { get; } = new DeclaredMessageExchangerFactory();

        internal DeclaredMessageExchanger(DeclaredMessageExchangerOptions options, IObjectStoreFactory objectStoreFactory,
                IConnectionController connectionController, INodeFinder nodeFinder, IPublishMessageStorage publishStorage, IWantMessageStorage wantStorage,
                IBytesPool bytesPool)
        {
            _options = options;
            _objectStoreFactory = objectStoreFactory;
            _connectionController = connectionController;
            _nodeFinder = nodeFinder;
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
            _objectStore = await _objectStoreFactory.CreateAsync(_options.ConfigPath, _bytesPool);

            await this.LoadAsync();

            _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
            _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
            _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
            _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_connectLoopTask, _acceptLoopTask, _sendLoopTask, _receiveLoopTask, _computeLoopTask);

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

        private async ValueTask<bool> TryAddConnectionAsync(IConnection connection, OmniAddress address, ConnectionHandshakeType handshakeType, OmniHash? rootHash, CancellationToken cancellationToken = default)
        {
            try
            {
                DeclaredMessageExchangerVersion? version = 0;
                {
                    var myHelloMessage = new DeclaredMessageExchangerHelloMessage(new[] { DeclaredMessageExchangerVersion.Version1 });
                    DeclaredMessageExchangerHelloMessage? otherHelloMessage = null;

                    var enqueueTask = connection.EnqueueAsync((bufferWriter) => myHelloMessage.Export(bufferWriter, _bytesPool), cancellationToken);
                    var dequeueTask = connection.DequeueAsync((sequence) => otherHelloMessage = DeclaredMessageExchangerHelloMessage.Import(sequence, _bytesPool), cancellationToken);

                    await ValueTaskHelper.WhenAll(enqueueTask, dequeueTask);
                    if (otherHelloMessage == null) throw new Exception();

                    version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
                }

                if (version == DeclaredMessageExchangerVersion.Version1)
                {
                    NodeProfile? nodeProfile = null;
                    {
                        {
                            var myNodeProflie = await _nodeFinder.GetMyNodeProfile();
                            var myProfileMessage = new DeclaredMessageExchangerProfileMessage(myNodeProflie);
                            DeclaredMessageExchangerProfileMessage? otherProfileMessage = null;

                            var enqueueTask = connection.EnqueueAsync((bufferWriter) => myProfileMessage.Export(bufferWriter, _bytesPool), cancellationToken);
                            var dequeueTask = connection.DequeueAsync((sequence) => otherProfileMessage = DeclaredMessageExchangerProfileMessage.Import(sequence, _bytesPool), cancellationToken);

                            await ValueTaskHelper.WhenAll(enqueueTask, dequeueTask);
                            if (otherProfileMessage == null) throw new Exception();
                            nodeProfile = otherProfileMessage.NodeProfile;
                        }

                        if (handshakeType == ConnectionHandshakeType.Connected)
                        {
                            var requestMessage = new DeclaredMessageExchangerRequestMessage(rootHash.Value);
                            await connection.EnqueueAsync((bufferWriter) => requestMessage.Export(bufferWriter, _bytesPool), cancellationToken);

                            DeclaredMessageExchangerResponseMessage? responseMessage = null;
                            await connection.DequeueAsync((sequence) => responseMessage = DeclaredMessageExchangerResponseMessage.Import(sequence, _bytesPool), cancellationToken);

                            if (responseMessage == null || !responseMessage.Accepted) throw new Exception();
                        }
                        else if (handshakeType == ConnectionHandshakeType.Accepted)
                        {
                            DeclaredMessageExchangerRequestMessage? requestMessage = null;
                            await connection.DequeueAsync((sequence) => requestMessage = DeclaredMessageExchangerRequestMessage.Import(sequence, _bytesPool), cancellationToken);
                            if (requestMessage == null) throw new Exception();

                            rootHash = requestMessage.RootHash;
                            bool accepted = _publishStorage.Contains(requestMessage.RootHash) || _wantStorage.Contains(requestMessage.RootHash);

                            var responseMessage = new DeclaredMessageExchangerResponseMessage(accepted);
                            await connection.EnqueueAsync((bufferWriter) => responseMessage.Export(bufferWriter, _bytesPool), cancellationToken);

                            if (!accepted) throw new Exception();
                        }
                    }

                    var status = new ConnectionStatus(connection, address, handshakeType, nodeProfile, rootHash.Value);

                    lock (_lockObject)
                    {
                        _connectionStatusSet.Add(status);
                    }

                    return true;
                }

                throw new NotSupportedException();
            }
            catch (Exception)
            {
                connection.Dispose();
            }

            return false;
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
                        int connectionCount = _connectionStatusSet.Select(n => n.HandshakeType == ConnectionHandshakeType.Connected).Count();

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    foreach (var tag in _wantStorage.GetWantTags())
                    {
                        NodeProfile? targetNodeProfile = null;
                        {
                            var nodeProfiles = await _nodeFinder.FindNodeProfiles(tag, cancellationToken);
                            random.Shuffle(nodeProfiles);

                            lock (_lockObject)
                            {
                                var ignoreAddressSet = new HashSet<OmniAddress>(_connectionStatusSet.Select(n => n.Address));

                                foreach (var nodeProfile in nodeProfiles)
                                {
                                    if (nodeProfile.Addresses.Any(n => ignoreAddressSet.Contains(n))) continue;
                                    targetNodeProfile = nodeProfile;
                                    break;
                                }
                            }
                        }

                        if (targetNodeProfile == null) continue;

                        foreach (var targetAddress in targetNodeProfile.Addresses)
                        {
                            var connection = await _connectionController.ConnectAsync(targetAddress, ServiceName, cancellationToken);
                            if (connection != null)
                            {
                                if (await this.TryAddConnectionAsync(connection, targetAddress, ConnectionHandshakeType.Connected, tag.Hash, cancellationToken))
                                {
                                    goto End;
                                }
                            }
                        }
                    }

                End:;
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
                        int connectionCount = _connectionStatusSet.Select(n => n.HandshakeType == ConnectionHandshakeType.Accepted).Count();

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    var result = await _connectionController.AcceptAsync(ServiceName, cancellationToken);
                    if (result.Connection != null && result.Address != null)
                    {
                        await this.TryAddConnectionAsync(result.Connection, result.Address, ConnectionHandshakeType.Accepted, null, cancellationToken);
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

        private async Task SendLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    foreach (var connectionStatus in _connectionStatusSet.ToArray())
                    {
                        lock (connectionStatus.LockObject)
                        {
                            if (connectionStatus.SendingDataMessage != null)
                            {
                                connectionStatus.Connection.TryEnqueue(bufferWriter =>
                                {
                                    connectionStatus.SendingDataMessage.Export(bufferWriter, _bytesPool);
                                    connectionStatus.SendingDataMessage = null;
                                });
                            }
                        }
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

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    foreach (var connectionStatus in _connectionStatusSet.ToArray())
                    {
                        DeclaredMessageExchangerDataMessage? dataMessage = null;

                        connectionStatus.Connection.TryDequeue((sequence) =>
                        {
                            dataMessage = DeclaredMessageExchangerDataMessage.Import(sequence, _bytesPool);
                        });

                        if (dataMessage == null)
                        {
                            continue;
                        }

                        lock (connectionStatus.LockObject)
                        {
                            connectionStatus.ReceivedContentBlockFlags = dataMessage.ContentBlockFlags.ToArray();
                            connectionStatus.ReceivedWantContentBlocks.Clear();
                            connectionStatus.ReceivedWantContentBlocks.AddRange(dataMessage.WantContentBlocks);
                        }

                        foreach (var comessageock in dataMessage.GiveContentBlocks)
                        {
                            await _wantStorage.WriteAsync(connectionStatus.RootHash, comessageock.Tag, comessageock.Value, cancellationToken);
                        }
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

        private sealed class ComputingNodeElement
        {
            public ComputingNodeElement(ConnectionStatus connectionStatus)
            {
                this.ConnectionStatus = connectionStatus;
            }

            public ConnectionStatus ConnectionStatus { get; }

            public List<NodeProfile> SendingPushNodeProfiles { get; } = new List<NodeProfile>();
            public Dictionary<Tag, List<NodeProfile>> SendingPushLocations { get; } = new Dictionary<Tag, List<NodeProfile>>();
            public List<Tag> SendingWantLocations { get; } = new List<Tag>();
            public Dictionary<Tag, List<NodeProfile>> SendingGiveLocations { get; } = new Dictionary<Tag, List<NodeProfile>>();
        }

        private async Task ComputeLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    lock (_lockObject)
                    {
                        foreach (var connectionStatus in _connectionStatusSet)
                        {
                            connectionStatus.Refresh();
                        }
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


        private enum ConnectionHandshakeType
        {
            Connected,
            Accepted,
        }

        private sealed class ConnectionStatus : ISynchronized
        {
            public ConnectionStatus(IConnection connection, OmniAddress address,
                ConnectionHandshakeType handshakeType, NodeProfile nodeProfile, OmniHash rootHash)
            {
                this.Connection = connection;
                this.Address = address;
                this.HandshakeType = handshakeType;
                this.NodeProfile = nodeProfile;
                this.RootHash = rootHash;
            }

            public object LockObject { get; } = new object();

            public IConnection Connection { get; }
            public OmniAddress Address { get; }
            public ConnectionHandshakeType HandshakeType { get; }

            public NodeProfile NodeProfile { get; }
            public OmniHash RootHash { get; }

            public DeclaredMessageExchangerDataMessage? SendingDataMessage { get; set; } = null;
            public ContentBlockFlags[]? ReceivedContentBlockFlags { get; set; } = null;
            public List<OmniHash> ReceivedWantContentBlocks { get; } = new List<OmniHash>();

            public void Refresh()
            {

            }
        }
    }
}
