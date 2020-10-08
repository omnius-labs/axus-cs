using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Helpers;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Connections.Extensions;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Engines.Internal;
using Omnius.Xeus.Engines.Mediators.Internal.Models;
using Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Engines.Engines
{
    public sealed class CkadMediator : AsyncDisposableBase, ICkadMediator
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly CkadMediatorOptions _options;
        private readonly List<IConnector> _connectors = new List<IConnector>();
        private readonly IBytesPool _bytesPool;

        private readonly ReadOnlyMemory<byte> _myId;

        private readonly HashSet<ConnectionStatus> _connections = new HashSet<ConnectionStatus>();
        private readonly LinkedList<NodeProfile> _cloudNodeProfiles = new LinkedList<NodeProfile>();

        private readonly VolatileListDictionary<ResourceTag, NodeProfile> _receivedPushLocationMap = new VolatileListDictionary<ResourceTag, NodeProfile>(TimeSpan.FromMinutes(30));
        private readonly VolatileListDictionary<ResourceTag, NodeProfile> _receivedGiveLocationMap = new VolatileListDictionary<ResourceTag, NodeProfile>(TimeSpan.FromMinutes(30));

        private Task? _connectLoopTask;
        private Task? _acceptLoopTask;
        private Task? _sendLoopTask;
        private Task? _receiveLoopTask;
        private Task? _computeLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly object _lockObject = new object();

        private const int MaxBucketLength = 20;

        public event GetAvailableEngineNames? GetAvailableEngineNames;
        public event GetFetchResourceTags? GetPushFetchResourceTags;
        public event GetFetchResourceTags? GetWantFetchResourceTags;

        internal sealed class CkadMediatorFactory : ICkadMediatorFactory
        {
            public async ValueTask<ICkadMediator> CreateAsync(CkadMediatorOptions options, IEnumerable<IConnector> connectors, IBytesPool bytesPool)
            {
                var result = new CkadMediator(options, connectors, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public string EngineName => "node-explorer";

        public static ICkadMediatorFactory Factory { get; } = new CkadMediatorFactory();

        internal CkadMediator(CkadMediatorOptions options, IEnumerable<IConnector> connectors, IBytesPool bytesPool)
        {
            _options = options;
            _connectors.AddRange(connectors);
            _bytesPool = bytesPool;
            _myId = GenId();
        }

        private static byte[] GenId()
        {
            var id = new byte[32];
            using var random = RandomNumberGenerator.Create();
            random.GetBytes(id);
            return id;
        }

        public async ValueTask InitAsync()
        {
            _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
            _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
            _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
            _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_connectLoopTask!, _acceptLoopTask!, _sendLoopTask!, _receiveLoopTask!, _computeLoopTask!);

            _cancellationTokenSource.Dispose();
        }

        private IEnumerable<string> OnGetAvailableEngineNames()
        {
            var results = new ConcurrentBag<string>();
            this.GetAvailableEngineNames?.Invoke((name) => results.Add(name));
            return results;
        }

        private IEnumerable<ResourceTag> OnGetPushFetchResourceTags()
        {
            var results = new ConcurrentBag<ResourceTag>();
            this.GetPushFetchResourceTags?.Invoke((tag) => results.Add(tag));
            return results;
        }

        private IEnumerable<ResourceTag> OnGetWantFetchResourceTags()
        {
            var results = new ConcurrentBag<ResourceTag>();
            this.GetWantFetchResourceTags?.Invoke((tag) => results.Add(tag));
            return results;
        }

        public async ValueTask<NodeProfile[]> FindNodeProfilesAsync(ResourceTag tag, CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
                var result = new HashSet<NodeProfile>();

                if (_receivedPushLocationMap.TryGetValue(tag, out var nodeProfiles1))
                {
                    result.UnionWith(nodeProfiles1);
                }

                if (_receivedGiveLocationMap.TryGetValue(tag, out var nodeProfiles2))
                {
                    result.UnionWith(nodeProfiles2);
                }

                return result.ToArray();
            }
        }

        public async ValueTask<NodeProfile> GetMyNodeProfileAsync(CancellationToken cancellationToken = default)
        {
            var addresses = new List<OmniAddress>();
            foreach (var connector in _connectors)
            {
                addresses.AddRange(await connector.GetListenEndpointsAsync(cancellationToken));
            }

            var services = new List<string>();
            services.Add(this.EngineName);
            services.AddRange(this.OnGetAvailableEngineNames().ToArray());

            var myNodeProflie = new NodeProfile(services.ToArray(), addresses.ToArray());
            return myNodeProflie;
        }

        public async ValueTask AddCloudNodeProfilesAsync(IEnumerable<NodeProfile> nodeProfiles, CancellationToken cancellationToken = default)
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

        private readonly VolatileHashSet<OmniAddress> _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromMinutes(30));

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

                    NodeProfile? targetNodeProfile = null;

                    lock (_lockObject)
                    {
                        var nodeProfiles = _cloudNodeProfiles.ToArray();
                        random.Shuffle(nodeProfiles);

                        var ignoreAddressSet = new HashSet<OmniAddress>();
                        ignoreAddressSet.Union(_connections.Select(n => n.Address));
                        ignoreAddressSet.Union(_connectedAddressSet);

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
                        IConnection? connection = null;

                        foreach (var connector in _connectors)
                        {
                            connection = await connector.ConnectAsync(targetAddress, this.EngineName, cancellationToken);
                            if (connection == null) continue;
                        }

                        if (connection == null) continue;

                        _connectedAddressSet.Add(targetAddress);

                        if (await this.TryAddConnectionAsync(connection, targetAddress, ConnectionHandshakeType.Connected, cancellationToken))
                        {
                            break;
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
                        int connectionCount = _connections.Select(n => n.HandshakeType == ConnectionHandshakeType.Accepted).Count();

                        if (_connections.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    foreach (var connector in _connectors)
                    {
                        var result = await connector.AcceptAsync(this.EngineName, cancellationToken);
                        if (result.Connection != null && result.Address != null)
                        {
                            await this.TryAddConnectionAsync(result.Connection, result.Address, ConnectionHandshakeType.Accepted, cancellationToken);
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

        private async ValueTask<bool> TryAddConnectionAsync(IConnection connection, OmniAddress address, ConnectionHandshakeType handshakeType, CancellationToken cancellationToken = default)
        {
            // kademliaのk-bucketの距離毎のノード数は最大20とする。(k=20)
            bool CanAppend(ReadOnlySpan<byte> id)
            {
                lock (_lockObject)
                {
                    var appendingNodeDistance = Kademlia.Distance(_myId.Span, id);

                    var map = new Dictionary<int, int>();
                    foreach (var connectionStatus in _connections)
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
                CkadMediatorVersion? version = 0;
                {
                    var myHelloMessage = new CkadMediatorHelloMessage(new[] { CkadMediatorVersion.Version1 });

                    var enqueueTask = connection.EnqueueAsync(myHelloMessage, cancellationToken).AsTask();
                    var dequeueTask = connection.DequeueAsync<CkadMediatorHelloMessage>(cancellationToken).AsTask();
                    await Task.WhenAll(enqueueTask, dequeueTask);

                    var otherHelloMessage = dequeueTask.Result;
                    if (otherHelloMessage == null) throw new Exception();

                    version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
                }

                if (version == CkadMediatorVersion.Version1)
                {
                    ReadOnlyMemory<byte> id;
                    NodeProfile? nodeProfile = null;
                    {
                        var myNodeProflie = await this.GetMyNodeProfileAsync();
                        var myProfileMessage = new CkadMediatorProfileMessage(_myId, myNodeProflie);

                        var enqueueTask = connection.EnqueueAsync(myProfileMessage, cancellationToken).AsTask();
                        var dequeueTask = connection.DequeueAsync<CkadMediatorProfileMessage>(cancellationToken).AsTask();
                        await Task.WhenAll(enqueueTask, dequeueTask);

                        var otherProfileMessage = dequeueTask.Result;
                        if (otherProfileMessage == null) throw new Exception();

                        id = otherProfileMessage.Id;
                        nodeProfile = otherProfileMessage.NodeProfile;
                    }

                    if (!CanAppend(id.Span)) throw new Exception();

                    var status = new ConnectionStatus(connection, address, handshakeType, nodeProfile, id);

                    lock (_lockObject)
                    {
                        _connections.Add(status);
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

        private async Task SendLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    foreach (var status in _connections.ToArray())
                    {
                        try
                        {
                            lock (status.LockObject)
                            {
                                if (status.SendingDataMessage != null)
                                {
                                    if (status.Connection.TryEnqueue(status.SendingDataMessage))
                                    {
                                        status.SendingDataMessage = null;
                                    }
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            lock (_lockObject)
                            {
                                _connections.Remove(status);
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

                    foreach (var status in _connections.ToArray())
                    {
                        try
                        {
                            if (status.Connection.TryDequeue<CkadMediatorDataMessage>(out var dataMessage))
                            {
                                await this.AddCloudNodeProfilesAsync(dataMessage.PushNodeProfiles, cancellationToken);

                                lock (status.LockObject)
                                {
                                    status.ReceivedWantLocations.UnionWith(dataMessage.WantResourceLocations);
                                }

                                lock (_lockObject)
                                {
                                    foreach (var contentLocation in dataMessage.PushResourceLocations)
                                    {
                                        _receivedPushLocationMap.AddRange(contentLocation.ResourceTag, contentLocation.NodeProfiles);
                                    }

                                    foreach (var contentLocation in dataMessage.GiveResourceLocations)
                                    {
                                        _receivedGiveLocationMap.AddRange(contentLocation.ResourceTag, contentLocation.NodeProfiles);
                                    }
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            lock (_lockObject)
                            {
                                _connections.Remove(status);
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
            public Dictionary<ResourceTag, List<NodeProfile>> SendingPushLocations { get; } = new Dictionary<ResourceTag, List<NodeProfile>>();
            public List<ResourceTag> SendingWantLocations { get; } = new List<ResourceTag>();
            public Dictionary<ResourceTag, List<NodeProfile>> SendingGiveLocations { get; } = new Dictionary<ResourceTag, List<NodeProfile>>();
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

                        foreach (var connectionStatus in _connections)
                        {
                            connectionStatus.Refresh();
                        }
                    }

                    // 自分のノードプロファイル
                    var myNodeProfile = await this.GetMyNodeProfileAsync(cancellationToken);

                    // ノード情報
                    var elements = new List<KademliaElement<ComputingNodeElement>>();

                    // 送信するノードプロファイル
                    var sendingPushNodeProfiles = new List<NodeProfile>();

                    // コンテンツのロケーション情報
                    var contentLocationMap = new Dictionary<ResourceTag, HashSet<NodeProfile>>();

                    // 送信するコンテンツのロケーション情報
                    var sendingPushLocationMap = new Dictionary<ResourceTag, HashSet<NodeProfile>>();

                    // 送信するコンテンツのロケーションリクエスト情報
                    var sendingWantLocationSet = new HashSet<ResourceTag>();

                    lock (_lockObject)
                    {
                        foreach (var connectionStatus in _connections)
                        {
                            elements.Add(new KademliaElement<ComputingNodeElement>(connectionStatus.Id, new ComputingNodeElement(connectionStatus)));
                        }
                    }

                    lock (_lockObject)
                    {
                        sendingPushNodeProfiles.AddRange(_cloudNodeProfiles);
                    }

                    foreach (var tag in this.OnGetPushFetchResourceTags())
                    {
                        contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                             .Add(myNodeProfile);

                        sendingPushLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                             .Add(myNodeProfile);
                    }

                    foreach (var tag in this.OnGetWantFetchResourceTags())
                    {
                        sendingWantLocationSet.Add(tag);
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
                                new CkadMediatorDataMessage(
                                    element.Value.SendingPushNodeProfiles.Take(CkadMediatorDataMessage.MaxPushNodeProfilesCount).ToArray(),
                                    element.Value.SendingPushLocations.Select(n => new ResourceLocation(n.Key, n.Value.Take(ResourceLocation.MaxNodeProfilesCount).ToArray())).Take(CkadMediatorDataMessage.MaxPushResourceLocationsCount).ToArray(),
                                    element.Value.SendingWantLocations.Take(CkadMediatorDataMessage.MaxWantResourceLocationsCount).ToArray(),
                                    element.Value.SendingGiveLocations.Select(n => new ResourceLocation(n.Key, n.Value.Take(ResourceLocation.MaxNodeProfilesCount).ToArray())).Take(CkadMediatorDataMessage.MaxGiveResourceLocationsCount).ToArray());
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

            public CkadMediatorDataMessage? SendingDataMessage { get; set; } = null;
            public VolatileHashSet<ResourceTag> ReceivedWantLocations { get; } = new VolatileHashSet<ResourceTag>(TimeSpan.FromMinutes(30));

            public void Refresh()
            {
                this.ReceivedWantLocations.Refresh();
            }
        }
    }
}
