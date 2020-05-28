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

        private readonly HashSet<ConnectionStatus> _connectionStatusSet = new HashSet<ConnectionStatus>();
        private readonly LinkedList<NodeProfile> _cloudNodeProfiles = new LinkedList<NodeProfile>();

        private readonly VolatileListDictionary<OmniHash, NodeProfile> _receivedPushContentLocationMap = new VolatileListDictionary<OmniHash, NodeProfile>(TimeSpan.FromMinutes(30));
        private readonly VolatileListDictionary<OmniHash, NodeProfile> _receivedGiveContentLocationMap = new VolatileListDictionary<OmniHash, NodeProfile>(TimeSpan.FromMinutes(30));

        private Task _connectLoopTask;
        private Task _acceptLoopTask;
        private Task _sendLoopTask;
        private Task _receiveLoopTask;
        private Task _computeLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly object _lockObject = new object();

        public const string ServiceName = "NodeExplorer`";

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
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
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

        public async ValueTask<NodeProfile[]> FindNodeProfiles(OmniHash tag, CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
                var result = new HashSet<NodeProfile>();

                {
                    if (_receivedPushContentLocationMap.TryGetValue(tag, out var nodeProfiles))
                    {
                        result.UnionWith(nodeProfiles);
                    }
                }

                {
                    if (_receivedGiveContentLocationMap.TryGetValue(tag, out var nodeProfiles))
                    {
                        result.UnionWith(nodeProfiles);
                    }
                }

                return result.ToArray();
            }
        }

        public async ValueTask<NodeProfile> GetMyNodeProfile(CancellationToken cancellationToken = default)
        {
            var addresses = await _connectionController.GetListenEndpointsAsync(cancellationToken);
            var services = new[] { NodeExplorer.ServiceName, BlockExchanger.ServiceName };
            var myNodeProflie = new NodeProfile(addresses, services);
            return myNodeProflie;
        }

        public async ValueTask AddCloudNodeProfiles(IEnumerable<NodeProfile> nodeProfiles, CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
                foreach (var nodeProfile in nodeProfiles)
                {
                    if (_cloudNodeProfiles.Count >= 2048)
                    {
                        return;
                    }

                    _cloudNodeProfiles.AddLast(nodeProfile);
                }
            }
        }

        private void RefreshCloudNodeProfile(NodeProfile nodeProfile)
        {
            lock (_lockObject)
            {
                _cloudNodeProfiles.RemoveAll(n => n.Addresses.Any(m => nodeProfile.Addresses.Contains(m)));
                _cloudNodeProfiles.AddFirst(nodeProfile);
            }
        }

        private bool RemoveCloudNodeProfile(NodeProfile nodeProfile)
        {
            lock (_lockObject)
            {
                if (_cloudNodeProfiles.Count >= 1024)
                {
                    return _cloudNodeProfiles.Remove(nodeProfile);
                }

                return false;
            }
        }

        private async ValueTask<bool> TryAddConnectionAsync(IConnection connection, OmniAddress address, ConnectionHandshakeType handshakeType, CancellationToken cancellationToken = default)
        {
            // kademliaのk-bucketの距離毎のノード数は最大20とする。(k=20)
            bool CanAppend(ReadOnlySpan<byte> id)
            {
                lock (_lockObject)
                {
                    var appendingNodeDistance = RouteTable.Distance(_myId.Span, id);

                    var map = new Dictionary<int, int>();
                    foreach (var connectionStatus in _connectionStatusSet)
                    {
                        var nodeDistance = RouteTable.Distance(_myId.Span, id);
                        map.TryGetValue(nodeDistance, out int count);
                        count++;
                        map[nodeDistance] = count;
                    }

                    {
                        map.TryGetValue(appendingNodeDistance, out int count);
                        if (count > 20)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            try
            {
                NodeExplorerVersion? version = 0;
                {
                    var myHelloMessage = new NodeExplorerHelloMessage(new[] { NodeExplorerVersion.Version1 });
                    NodeExplorerHelloMessage? otherHelloMessage = null;

                    var enqueueTask = connection.EnqueueAsync((bufferWriter) => myHelloMessage.Export(bufferWriter, _bytesPool), cancellationToken);
                    var dequeueTask = connection.DequeueAsync((sequence) => otherHelloMessage = NodeExplorerHelloMessage.Import(sequence, _bytesPool), cancellationToken);

                    await ValueTaskHelper.WhenAll(enqueueTask, dequeueTask);
                    if (otherHelloMessage == null) throw new Exception();

                    version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
                }

                if (version == NodeExplorerVersion.Version1)
                {
                    ReadOnlyMemory<byte> id;
                    NodeProfile? nodeProfile = null;
                    {
                        var addresses = await _connectionController.GetListenEndpointsAsync(cancellationToken);
                        var services = new[] { NodeExplorer.ServiceName, BlockExchanger.ServiceName };
                        var myNodeProflie = new NodeProfile(addresses, services);

                        var myProfileMessage = new NodeExplorerProfileMessage(_myId, myNodeProflie);
                        NodeExplorerProfileMessage? otherProfileMessage = null;

                        var enqueueTask = connection.EnqueueAsync((bufferWriter) => myProfileMessage.Export(bufferWriter, _bytesPool), cancellationToken);
                        var dequeueTask = connection.DequeueAsync((sequence) => otherProfileMessage = NodeExplorerProfileMessage.Import(sequence, _bytesPool), cancellationToken);

                        await ValueTaskHelper.WhenAll(enqueueTask, dequeueTask);
                        if (otherProfileMessage == null) throw new Exception();

                        id = otherProfileMessage.Id;
                        nodeProfile = otherProfileMessage.NodeProfile;
                    }

                    if (!CanAppend(id.Span))
                    {
                        throw new Exception();
                    }

                    var status = new ConnectionStatus(connection, address, handshakeType, nodeProfile, id);

                    lock (_lockObject)
                    {
                        _connectionStatusSet.Add(status);
                    }

                    if (status.HandshakeType == ConnectionHandshakeType.Connected)
                    {
                        this.RefreshCloudNodeProfile(status.NodeProfile);
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

                    NodeProfile? targetNodeProfile = null;

                    lock (_lockObject)
                    {
                        var nodeProfiles = _cloudNodeProfiles.ToArray();
                        random.Shuffle(nodeProfiles);

                        var ignoreAddressSet = new HashSet<OmniAddress>(_connectionStatusSet.Select(n => n.Address));

                        foreach (var nodeProfile in nodeProfiles)
                        {
                            if (nodeProfile.Addresses.Any(n => ignoreAddressSet.Contains(n))) continue;
                            targetNodeProfile = nodeProfile;
                            break;
                        }
                    }

                    if (targetNodeProfile == null) continue;

                    bool succeeded = false;

                    foreach (var targetAddress in targetNodeProfile.Addresses)
                    {
                        var connection = await _connectionController.ConnectAsync(targetAddress, ServiceName, cancellationToken);
                        if (connection != null)
                        {
                            if (await this.TryAddConnectionAsync(connection, targetAddress, ConnectionHandshakeType.Connected, cancellationToken))
                            {
                                succeeded = true;
                                break;
                            }
                        }
                    }

                    if (succeeded)
                    {
                        this.RefreshCloudNodeProfile(targetNodeProfile);
                    }
                    else
                    {
                        this.RemoveCloudNodeProfile(targetNodeProfile);
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
                        int connectionCount = _connectionStatusSet.Select(n => n.HandshakeType == ConnectionHandshakeType.Accepted).Count();

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    var result = await _connectionController.AcceptAsync(ServiceName, cancellationToken);
                    if (result.Connection != null && result.Address != null)
                    {
                        await this.TryAddConnectionAsync(result.Connection, result.Address, ConnectionHandshakeType.Accepted, cancellationToken);
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
                        NodeExplorerDataMessage? dataMessage = null;

                        connectionStatus.Connection.TryDequeue((sequence) =>
                        {
                            dataMessage = NodeExplorerDataMessage.Import(sequence, _bytesPool);
                        });

                        if (dataMessage == null)
                        {
                            continue;
                        }

                        await this.AddCloudNodeProfiles(dataMessage.PushNodeProfiles);

                        lock (connectionStatus.LockObject)
                        {
                            connectionStatus.ReceivedWantContentLocations.UnionWith(dataMessage.WantContentLocations);
                        }

                        lock (_lockObject)
                        {
                            foreach (var contentLocation in dataMessage.PushContentLocations)
                            {
                                _receivedPushContentLocationMap.AddRange(contentLocation.Tag, contentLocation.NodeProfiles);
                            }

                            foreach (var contentLocation in dataMessage.GiveContentLocations)
                            {
                                _receivedGiveContentLocationMap.AddRange(contentLocation.Tag, contentLocation.NodeProfiles);
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

        private sealed class ComputingNodeElement
        {
            public ComputingNodeElement(ConnectionStatus connectionStatus)
            {
                this.ConnectionStatus = connectionStatus;
            }

            public ConnectionStatus ConnectionStatus { get; }

            public List<NodeProfile> SendingPushNodeProfiles { get; } = new List<NodeProfile>();
            public Dictionary<OmniHash, List<NodeProfile>> SendingPushContentLocations { get; } = new Dictionary<OmniHash, List<NodeProfile>>();
            public List<OmniHash> SendingWantContentLocations { get; } = new List<OmniHash>();
            public Dictionary<OmniHash, List<NodeProfile>> SendingGiveContentLocations { get; } = new Dictionary<OmniHash, List<NodeProfile>>();
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
                        _receivedPushContentLocationMap.Refresh();
                        _receivedGiveContentLocationMap.Refresh();

                        foreach (var connectionStatus in _connectionStatusSet)
                        {
                            connectionStatus.Refresh();
                        }
                    }

                    // 自分のノードプロファイル
                    var myNodeProfile = await this.GetMyNodeProfile(cancellationToken);

                    // ノード情報
                    var elements = new List<RouteTableElement<ComputingNodeElement>>();

                    // 送信するノードプロファイル
                    var sendingPushNodeProfiles = new List<NodeProfile>();

                    // コンテンツのロケーション情報
                    var contentLocationMap = new Dictionary<OmniHash, HashSet<NodeProfile>>();

                    // 送信するコンテンツのロケーション情報
                    var sendingPushContentLocationMap = new Dictionary<OmniHash, HashSet<NodeProfile>>();

                    // 送信するコンテンツのロケーションリクエスト情報
                    var sendingWantContentLocationSet = new HashSet<OmniHash>();

                    lock (_lockObject)
                    {
                        foreach (var connectionStatus in _connectionStatusSet)
                        {
                            elements.Add(new RouteTableElement<ComputingNodeElement>(connectionStatus.Id, new ComputingNodeElement(connectionStatus)));
                        }
                    }

                    lock (_lockObject)
                    {
                        sendingPushNodeProfiles.AddRange(_cloudNodeProfiles);
                    }

                    foreach (var publishReport in await _publishStorage.GetReportsAsync(cancellationToken))
                    {
                        contentLocationMap.GetOrAdd(publishReport.Tag, (_) => new HashSet<NodeProfile>())
                             .Add(myNodeProfile);

                        sendingPushContentLocationMap.GetOrAdd(publishReport.Tag, (_) => new HashSet<NodeProfile>())
                             .Add(myNodeProfile);
                    }

                    foreach (var wantReport in await _wantStorage.GetReportsAsync(cancellationToken))
                    {
                        sendingWantContentLocationSet.Add(wantReport.Tag);
                    }

                    lock (_lockObject)
                    {
                        foreach (var (tag, nodeProfiles) in _receivedPushContentLocationMap)
                        {
                            contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);

                            sendingPushContentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);
                        }

                        foreach (var (tag, nodeProfiles) in _receivedGiveContentLocationMap)
                        {
                            contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);
                        }
                    }

                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            foreach (var tag in element.Value.ConnectionStatus.ReceivedWantContentLocations)
                            {
                                sendingWantContentLocationSet.Add(tag);
                            }
                        }
                    }

                    // Compute PushNodeProfiles
                    foreach (var element in elements)
                    {
                        element.Value.SendingPushNodeProfiles.AddRange(sendingPushNodeProfiles);
                    }

                    // Compute PushContentLocations
                    foreach (var (tag, nodeProfiles) in sendingPushContentLocationMap)
                    {
                        foreach (var element in RouteTable.Search(_myId.Span, tag.Value.Span, elements, 1))
                        {
                            element.Value.SendingPushContentLocations[tag] = nodeProfiles.ToList();
                        }
                    }

                    // Compute WantContentLocations
                    foreach (var tag in sendingWantContentLocationSet)
                    {
                        foreach (var element in RouteTable.Search(_myId.Span, tag.Value.Span, elements, 1))
                        {
                            element.Value.SendingWantContentLocations.Add(tag);
                        }
                    }

                    // Compute GiveContentLocations
                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            foreach (var tag in element.Value.ConnectionStatus.ReceivedWantContentLocations)
                            {
                                if (!contentLocationMap.TryGetValue(tag, out var nodeProfiles))
                                {
                                    continue;
                                }

                                element.Value.SendingGiveContentLocations[tag] = nodeProfiles.ToList();
                            }
                        }
                    }

                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            element.Value.ConnectionStatus.SendingDataMessage =
                                new NodeExplorerDataMessage(
                                    element.Value.SendingPushNodeProfiles.Take(NodeExplorerDataMessage.MaxPushNodeProfilesCount).ToArray(),
                                    element.Value.SendingPushContentLocations.Select(n => new ContentLocation(n.Key, n.Value.Take(ContentLocation.MaxNodeProfilesCount).ToArray())).Take(NodeExplorerDataMessage.MaxPushContentLocationsCount).ToArray(),
                                    element.Value.SendingWantContentLocations.Take(NodeExplorerDataMessage.MaxWantContentLocationsCount).ToArray(),
                                    element.Value.SendingGiveContentLocations.Select(n => new ContentLocation(n.Key, n.Value.Take(ContentLocation.MaxNodeProfilesCount).ToArray())).Take(NodeExplorerDataMessage.MaxGiveContentLocationsCount).ToArray());
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
                ConnectionHandshakeType handshakeType, NodeProfile nodeProfile, ReadOnlyMemory<byte> id)
            {
                this.Connection = connection;
                this.Address = address;
                this.HandshakeType = handshakeType;
                this.NodeProfile = nodeProfile;
                this.Id = id;
            }

            public object LockObject { get; } = new object();

            public IConnection Connection { get; }
            public OmniAddress Address { get; }
            public ConnectionHandshakeType HandshakeType { get; }

            public NodeProfile NodeProfile { get; }
            public ReadOnlyMemory<byte> Id { get; }

            public NodeExplorerDataMessage? SendingDataMessage { get; set; } = null;
            public VolatileSet<OmniHash> ReceivedWantContentLocations { get; } = new VolatileSet<OmniHash>(TimeSpan.FromMinutes(30));

            public void Refresh()
            {
                this.ReceivedWantContentLocations.Refresh();
            }
        }
    }
}
