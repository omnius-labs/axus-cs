using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines.Internal;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class BlockExchanger : AsyncDisposableBase, IBlockExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly BlockExchangerOptions _options;
        private readonly IObjectStoreFactory _objectStoreFactory;
        private readonly IConnectionController _connectionController;
        private readonly INodeExplorer _nodeExplorer;
        private readonly IPublishStorage _publishStorage;
        private readonly IWantStorage _wantStorage;
        private readonly IBytesPool _bytesPool;

        private IObjectStore _objectStore;

        private readonly HashSet<ConnectionStatus> _connectionStatusSet = new HashSet<ConnectionStatus>();

        private Task _connectLoopTask;
        private Task _acceptLoopTask;
        private Task _sendLoopTask;
        private Task _receiveLoopTask;
        private Task _computeLoopTask;

        private readonly Random _random = new Random();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly object _lockObject = new object();
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public const string ServiceName = "BlockExchanger`";

        internal sealed class BlockExchangerFactory : IBlockExchangerFactory
        {
            public async ValueTask<IBlockExchanger> CreateAsync(string configPath, BlockExchangerOptions options,
                IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
                INodeExplorer nodeExplorer, IPublishStorage publishStorage, IWantStorage wantStorage, IBytesPool bytesPool)
            {
                var result = new BlockExchanger(configPath, options, objectStoreFactory, connectionController, nodeExplorer, publishStorage, wantStorage, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IBlockExchangerFactory Factory { get; } = new BlockExchangerFactory();

        internal BlockExchanger(string configPath, BlockExchangerOptions options,
                IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
                INodeExplorer nodeExplorer, IPublishStorage publishStorage, IWantStorage wantStorage, IBytesPool bytesPool)
        {
            _configPath = configPath;
            _options = options;
            _objectStoreFactory = objectStoreFactory;
            _connectionController = connectionController;
            _nodeExplorer = nodeExplorer;
            _publishStorage = publishStorage;
            _wantStorage = wantStorage;
            _bytesPool = bytesPool;
        }

        internal async ValueTask InitAsync()
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

        private async ValueTask<bool> TryAddConnectionAsync(IConnection connection, OmniAddress address, ConnectionHandshakeType handshakeType, OmniHash tag, CancellationToken cancellationToken = default)
        {
            // kademliaのk-bucketの距離毎のノード数は最大20とする。(k=20)
            bool CanAppend(ReadOnlySpan<byte> id)
            {
                lock (_lockObject)
                {
                    var appendingNodeDistance = Kademlia.Distance(_myId.Span, id);

                    var map = new Dictionary<int, int>();
                    foreach (var connectionStatus in _connectionStatusSet)
                    {
                        var nodeDistance = Kademlia.Distance(_myId.Span, id);
                        map.TryGetValue(nodeDistance, out int count);
                        count++;
                        map[nodeDistance] = count;
                    }

                    {
                        map.TryGetValue(appendingNodeDistance, out int count);
                        if (count > MaxBucketLength)
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

                    var wantReports = await _wantStorage.GetReportsAsync();
                    _random.Shuffle(wantReports);

                    foreach (var wantReport in wantReports)
                    {
                        var nodeProfiles = await _nodeExplorer.FindNodeProfiles(BlockExchanger.ServiceName, wantReport.Tag, cancellationToken);
                        _random.Shuffle(nodeProfiles);

                        targetNodeProfile = nodeProfiles.FirstOrDefault();

                        if (targetNodeProfile != null)
                        {
                            break;
                        }
                    }

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
                    var elements = new List<KademliaElement<ComputingNodeElement>>();

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
                            elements.Add(new KademliaElement<ComputingNodeElement>(connectionStatus.Id, new ComputingNodeElement(connectionStatus)));
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
                        foreach (var element in Kademlia.Search(_myId.Span, tag.Value.Span, elements, 1))
                        {
                            element.Value.SendingPushContentLocations[tag] = nodeProfiles.ToList();
                        }
                    }

                    // Compute WantContentLocations
                    foreach (var tag in sendingWantContentLocationSet)
                    {
                        foreach (var element in Kademlia.Search(_myId.Span, tag.Value.Span, elements, 1))
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

        private static uint GolombCodedSetComputeHash(OmniHash omniHash)
        {
            uint result = 0;
            const int unit = sizeof(uint);

            int loopCount = omniHash.Value.Length / unit;
            for (int i = 0; i < loopCount; i++)
            {
                result ^= BinaryPrimitives.ReadUInt32BigEndian(omniHash.Value.Span.Slice(i * unit, unit));
            }

            return result;
        }

        private enum ConnectionHandshakeType
        {
            Connected,
            Accepted,
        }

        private sealed class ConnectionStatus : ISynchronized
        {
            public ConnectionStatus(IConnection connection, OmniAddress address,
                ConnectionHandshakeType handshakeType, NodeProfile nodeProfile, OmniHash tag)
            {
                this.Connection = connection;
                this.Address = address;
                this.HandshakeType = handshakeType;
                this.NodeProfile = nodeProfile;
                this.Tag = tag;
            }

            public object LockObject { get; } = new object();

            public IConnection Connection { get; }
            public OmniAddress Address { get; }
            public ConnectionHandshakeType HandshakeType { get; }

            public NodeProfile NodeProfile { get; }
            public OmniHash Tag { get; }

            public VolatileSet<OmniHash> ReceivedWantBlocks { get; } = new VolatileSet<OmniHash>(TimeSpan.FromMinutes(30));
            public GolombCodedSet<OmniHash>? OwnedBlocksFilter { get; set; }

            public void Refresh()
            {
                this.ReceivedWantBlocks.Refresh();
            }
        }
    }
}
