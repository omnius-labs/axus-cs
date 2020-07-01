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
    public sealed class NodeFinder : AsyncDisposableBase, INodeFinder
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly NodeFinderOptions _options;
        private readonly IObjectStoreFactory _objectStoreFactory;
        private readonly IConnectionController _connectionController;
        private readonly List<IPublishStorage> _publishStorages = new List<IPublishStorage>();
        private readonly List<IWantStorage> _wantStorages = new List<IWantStorage>();
        private readonly IBytesPool _bytesPool;

        private readonly ReadOnlyMemory<byte> _myId;
        private IObjectStore _objectStore;

        private readonly HashSet<ConnectionStatus> _connectionStatusSet = new HashSet<ConnectionStatus>();
        private readonly LinkedList<NodeProfile> _cloudNodeProfiles = new LinkedList<NodeProfile>();

        private readonly VolatileListDictionary<Tag, NodeProfile> _receivedPushLocationMap = new VolatileListDictionary<Tag, NodeProfile>(TimeSpan.FromMinutes(30));
        private readonly VolatileListDictionary<Tag, NodeProfile> _receivedGiveLocationMap = new VolatileListDictionary<Tag, NodeProfile>(TimeSpan.FromMinutes(30));

        private Task _connectLoopTask;
        private Task _acceptLoopTask;
        private Task _sendLoopTask;
        private Task _receiveLoopTask;
        private Task _computeLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly object _lockObject = new object();

        private const int MaxBucketLength = 20;

        public const string ServiceName = "node-explorer`";

        internal sealed class NodeFinderFactory : INodeFinderFactory
        {
            public async ValueTask<INodeFinder> CreateAsync(NodeFinderOptions options, IObjectStoreFactory objectStoreFactory,
                IConnectionController connectionController, IEnumerable<IPublishContentStorage> publishStorages, IEnumerable<IWantContentStorage> wantStorages, IBytesPool bytesPool)
            {
                var result = new NodeFinder(options, objectStoreFactory, connectionController, publishStorages, wantStorages, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static INodeFinderFactory Factory { get; } = new NodeFinderFactory();

        internal NodeFinder(NodeFinderOptions options, IObjectStoreFactory objectStoreFactory,
                IConnectionController connectionController, IEnumerable<IPublishContentStorage> publishStorages, IEnumerable<IWantContentStorage> wantStorages,
                IBytesPool bytesPool)
        {
            _options = options;
            _objectStoreFactory = objectStoreFactory;
            _connectionController = connectionController;
            _publishStorages.AddRange(publishStorages);
            _wantStorages.AddRange(wantStorages);
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

        public async ValueTask<NodeProfile[]> FindNodeProfiles(Tag tag, CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
                var result = new HashSet<NodeProfile>();

                {
                    if (_receivedPushLocationMap.TryGetValue(tag, out var nodeProfiles))
                    {
                        result.UnionWith(nodeProfiles);
                    }
                }

                {
                    if (_receivedGiveLocationMap.TryGetValue(tag, out var nodeProfiles))
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
            var services = _options.EnabledServices.ToArray();
            var myNodeProflie = new NodeProfile(services, addresses);
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
                NodeFinderVersion? version = 0;
                {
                    var myHelloMessage = new NodeFinderHelloMessage(new[] { NodeFinderVersion.Version1 });
                    NodeFinderHelloMessage? otherHelloMessage = null;

                    var enqueueTask = connection.EnqueueAsync((bufferWriter) => myHelloMessage.Export(bufferWriter, _bytesPool), cancellationToken);
                    var dequeueTask = connection.DequeueAsync((sequence) => otherHelloMessage = NodeFinderHelloMessage.Import(sequence, _bytesPool), cancellationToken);

                    await ValueTaskHelper.WhenAll(enqueueTask, dequeueTask);
                    if (otherHelloMessage == null) throw new Exception();

                    version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
                }

                if (version == NodeFinderVersion.Version1)
                {
                    ReadOnlyMemory<byte> id;
                    NodeProfile? nodeProfile = null;
                    {
                        var myNodeProflie = await this.GetMyNodeProfile();

                        var myProfileMessage = new NodeFinderProfileMessage(_myId, myNodeProflie);
                        NodeFinderProfileMessage? otherProfileMessage = null;

                        var enqueueTask = connection.EnqueueAsync((bufferWriter) => myProfileMessage.Export(bufferWriter, _bytesPool), cancellationToken);
                        var dequeueTask = connection.DequeueAsync((sequence) => otherProfileMessage = NodeFinderProfileMessage.Import(sequence, _bytesPool), cancellationToken);

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
                        NodeFinderDataMessage? dataMessage = null;

                        connectionStatus.Connection.TryDequeue((sequence) =>
                        {
                            dataMessage = NodeFinderDataMessage.Import(sequence, _bytesPool);
                        });

                        if (dataMessage == null)
                        {
                            continue;
                        }

                        await this.AddCloudNodeProfiles(dataMessage.PushNodeProfiles);

                        lock (connectionStatus.LockObject)
                        {
                            connectionStatus.ReceivedWantLocations.UnionWith(dataMessage.WantLocations);
                        }

                        lock (_lockObject)
                        {
                            foreach (var contentLocation in dataMessage.PushLocations)
                            {
                                _receivedPushLocationMap.AddRange(contentLocation.Tag, contentLocation.NodeProfiles);
                            }

                            foreach (var contentLocation in dataMessage.GiveLocations)
                            {
                                _receivedGiveLocationMap.AddRange(contentLocation.Tag, contentLocation.NodeProfiles);
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
                        _receivedPushLocationMap.Refresh();
                        _receivedGiveLocationMap.Refresh();

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
                    var contentLocationMap = new Dictionary<Tag, HashSet<NodeProfile>>();

                    // 送信するコンテンツのロケーション情報
                    var sendingPushLocationMap = new Dictionary<Tag, HashSet<NodeProfile>>();

                    // 送信するコンテンツのロケーションリクエスト情報
                    var sendingWantLocationSet = new HashSet<Tag>();

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

                    foreach (var publishStorage in _publishStorages)
                    {
                        foreach (var tag in await publishStorage.GetPublishTagsAsync(cancellationToken))
                        {
                            contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .Add(myNodeProfile);

                            sendingPushLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .Add(myNodeProfile);
                        }
                    }

                    foreach (var wantStorage in _wantStorages)
                    {
                        foreach (var tag in await wantStorage.GetWantTagsAsync(cancellationToken))
                        {
                            sendingWantLocationSet.Add(tag);
                        }
                    }

                    lock (_lockObject)
                    {
                        foreach (var (tag, nodeProfiles) in _receivedPushLocationMap)
                        {
                            contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);

                            sendingPushLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);
                        }

                        foreach (var (tag, nodeProfiles) in _receivedGiveLocationMap)
                        {
                            contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);
                        }
                    }

                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            foreach (var tag in element.Value.ConnectionStatus.ReceivedWantLocations)
                            {
                                sendingWantLocationSet.Add(tag);
                            }
                        }
                    }

                    // Compute PushNodeProfiles
                    foreach (var element in elements)
                    {
                        element.Value.SendingPushNodeProfiles.AddRange(sendingPushNodeProfiles);
                    }

                    // Compute PushLocations
                    foreach (var (tag, nodeProfiles) in sendingPushLocationMap)
                    {
                        foreach (var element in Kademlia.Search(_myId.Span, tag.Hash.Value.Span, elements, 1))
                        {
                            element.Value.SendingPushLocations[tag] = nodeProfiles.ToList();
                        }
                    }

                    // Compute WantLocations
                    foreach (var tag in sendingWantLocationSet)
                    {
                        foreach (var element in Kademlia.Search(_myId.Span, tag.Hash.Value.Span, elements, 1))
                        {
                            element.Value.SendingWantLocations.Add(tag);
                        }
                    }

                    // Compute GiveLocations
                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            foreach (var tag in element.Value.ConnectionStatus.ReceivedWantLocations)
                            {
                                if (!contentLocationMap.TryGetValue(tag, out var nodeProfiles))
                                {
                                    continue;
                                }

                                element.Value.SendingGiveLocations[tag] = nodeProfiles.ToList();
                            }
                        }
                    }

                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            element.Value.ConnectionStatus.SendingDataMessage =
                                new NodeFinderDataMessage(
                                    element.Value.SendingPushNodeProfiles.Take(NodeFinderDataMessage.MaxPushNodeProfilesCount).ToArray(),
                                    element.Value.SendingPushLocations.Select(n => new Location(n.Key, n.Value.Take(Location.MaxNodeProfilesCount).ToArray())).Take(NodeFinderDataMessage.MaxPushLocationsCount).ToArray(),
                                    element.Value.SendingWantLocations.Take(NodeFinderDataMessage.MaxWantLocationsCount).ToArray(),
                                    element.Value.SendingGiveLocations.Select(n => new Location(n.Key, n.Value.Take(Location.MaxNodeProfilesCount).ToArray())).Take(NodeFinderDataMessage.MaxGiveLocationsCount).ToArray());
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

            public NodeFinderDataMessage? SendingDataMessage { get; set; } = null;
            public VolatileSet<Tag> ReceivedWantLocations { get; } = new VolatileSet<Tag>(TimeSpan.FromMinutes(30));

            public void Refresh()
            {
                this.ReceivedWantLocations.Refresh();
            }
        }
    }
}
